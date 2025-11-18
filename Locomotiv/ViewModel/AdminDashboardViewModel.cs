using GMap.NET;
using Locomotiv.Model;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace Locomotiv.ViewModel
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        // ===============================
        // Classe interne : état d'une simulation de train
        // ===============================
        private class SimulationTrainState
        {
            public Train Train { get; set; } = null!;
            public Itineraire Itineraire { get; set; } = null!;
            public List<Etape> Etapes { get; set; } = new();
            public List<PointArret> PointsArret { get; set; } = new();
            public int IndexSegment { get; set; }
            public DateTime DepartSegment { get; set; }
            public DateTime ArriveeSegment { get; set; }
            public PointLatLng PositionCourante { get; set; }
        }

        // Tous les trains en simulation, indexés par Id
        private readonly Dictionary<int, SimulationTrainState> _simulationsActives = new();

        // ===============================
        // Champs privés (Dépendances)
        // ===============================

        private readonly ITrainDAL _trainDAL;
        private readonly IStationDAL _stationDAL;
        private readonly IDialogService _dialogService;
        private readonly IBlockDAL _blockDAL;
        private readonly IPointArretDAL _pointArretDAL;
        private readonly IItineraireDAL _itineraireDAL;
        private readonly DispatcherTimer _simulationTimer;

        // ===============================
        // Collections exposées à la Vue
        // ===============================

        public ObservableCollection<Block> Blocks { get; private set; } = new();
        public ObservableCollection<Train> Trains { get; private set; } = new();
        public ObservableCollection<Station> Stations { get; private set; } = new();
        public ObservableCollection<PointArret> PointsInteret { get; private set; } = new();

        /// <summary>
        /// Points éventuellement utilisés pour tracer une polyline d’itinéraire.
        /// </summary>
        public ObservableCollection<PointLatLng> ItineraireCourantPoints { get; } = new();

        private ObservableCollection<string> _conflitsTextuels = new();
        public ObservableCollection<string> ConflitsTextuels
        {
            get => _conflitsTextuels;
            set
            {
                _conflitsTextuels = value;
                OnPropertyChanged(nameof(ConflitsTextuels));
            }
        }

        // ===============================
        // Propriétés dérivées
        // ===============================

        /// <summary>
        /// Permet à la vue de connaître la position des trains en simulation.
        /// </summary>
        public IEnumerable<(int TrainId, PointLatLng Position)> SimulationsActives =>
            _simulationsActives.Values.Select(s => (s.Train.Id, s.PositionCourante));

        public IEnumerable<Station> GetStations() => Stations;

        public IEnumerable<Train> GetTrainsEnMouvement() => TrainsEnMouvement;

        public ObservableCollection<Train> TrainsEnMouvement =>
            new ObservableCollection<Train>(Trains.Where(t => t.Etat == EtatTrain.EnTransit));

        public ObservableCollection<Train> TrainsDeLaStationSelectionnee =>
            new ObservableCollection<Train>(
                Trains.Where(t => t.Station?.Id == StationSelectionnee?.Id)
            );

        public ObservableCollection<Train> ArriveesDeLaStationSelectionnee =>
            new ObservableCollection<Train>(
                Trains.Where(t =>
                    t.Station?.Id == StationSelectionnee?.Id &&
                    t.Etat == EtatTrain.EnGare)
            );

        public ObservableCollection<Train> DepartsDeLaStationSelectionnee =>
            new ObservableCollection<Train>(
                Trains.Where(t =>
                    t.Station?.Id == StationSelectionnee?.Id &&
                    t.Etat == EtatTrain.EnAttente)
            );

        public ObservableCollection<string> ConflitsDeLaStationSelectionnee =>
            new ObservableCollection<string>(
                GetConflits()
                    .Where(c =>
                        c.TrainA.Station?.Id == StationSelectionnee?.Id ||
                        c.TrainB.Station?.Id == StationSelectionnee?.Id)
                    .Select(c => $"⚠️ {c.TrainA.Nom} et {c.TrainB.Nom} sur {c.BlockConflit.Nom}")
            );

        // ===============================
        // Propriétés bindées (sélections)
        // ===============================

        private Train? _selectedTrain;
        public Train? SelectedTrain
        {
            get => _selectedTrain;
            set
            {
                _selectedTrain = value;
                OnPropertyChanged(nameof(SelectedTrain));
            }
        }

        private Station? _stationSelectionnee;
        public Station? StationSelectionnee
        {
            get => _stationSelectionnee;
            set
            {
                _stationSelectionnee = value;
                OnPropertyChanged(nameof(StationSelectionnee));
                OnPropertyChanged(nameof(TrainsDeLaStationSelectionnee));
                OnPropertyChanged(nameof(ConflitsDeLaStationSelectionnee));
                OnPropertyChanged(nameof(ArriveesDeLaStationSelectionnee));
                OnPropertyChanged(nameof(DepartsDeLaStationSelectionnee));
            }
        }

        private PointArret? _pointArretSelectionne;

        /// <summary>
        /// Utilisé par le code-behind (markers sur la carte).
        /// </summary>
        public PointArret? PointArretSelectionne
        {
            get => _pointArretSelectionne;
            set
            {
                _pointArretSelectionne = value;
                OnPropertyChanged(nameof(PointArretSelectionne));
                OnPropertyChanged(nameof(PointInteretSelectionne));
            }
        }

        /// <summary>
        /// Utilisé par le XAML.
        /// </summary>
        public PointArret? PointInteretSelectionne
        {
            get => _pointArretSelectionne;
            set
            {
                _pointArretSelectionne = value;
                OnPropertyChanged(nameof(PointInteretSelectionne));
                OnPropertyChanged(nameof(PointArretSelectionne));
            }
        }

        // ===============================
        // Commandes
        // ===============================

        public ICommand AjouterTrainCommand { get; }
        public ICommand SupprimerTrainCommand { get; }
        public ICommand PlanifierItineraireCommand { get; }

        // ===============================
        // Constructeur
        // ===============================

        public AdminDashboardViewModel(
            ITrainDAL trainDAL,
            IDialogService dialogService,
            IStationDAL station,
            IBlockDAL block,
            IPointArretDAL pointArretDAL,
            IItineraireDAL itineraireDAL)
        {
            _trainDAL = trainDAL;
            _dialogService = dialogService;
            _stationDAL = station;
            _blockDAL = block;
            _pointArretDAL = pointArretDAL;
            _itineraireDAL = itineraireDAL;

            AjouterTrainCommand = new RelayCommand(AddTrain);
            SupprimerTrainCommand = new RelayCommand(DeleteTrain);
            PlanifierItineraireCommand = new RelayCommand(PlanifierItineraire);

            LoadStations();
            LoadTrains();
            LoadBlocks();
            LoadPointsInteret();

            _simulationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _simulationTimer.Tick += SimulationTimer_Tick;
        }

        // ===============================
        // Chargement des données
        // ===============================

        private void LoadStations()
        {
            Stations = new ObservableCollection<Station>(_stationDAL.GetAllStations());
            OnPropertyChanged(nameof(Stations));
        }

        private void LoadPointsInteret()
        {
            PointsInteret = new ObservableCollection<PointArret>(_pointArretDAL.GetAllPointArrets());
            OnPropertyChanged(nameof(PointsInteret));
        }

        private void LoadTrains()
        {
            Trains = new ObservableCollection<Train>(_trainDAL.GetAllTrains());
            OnPropertyChanged(nameof(Trains));
            OnPropertyChanged(nameof(TrainsEnMouvement));
            OnPropertyChanged(nameof(TrainsDeLaStationSelectionnee));
            OnPropertyChanged(nameof(ArriveesDeLaStationSelectionnee));
            OnPropertyChanged(nameof(DepartsDeLaStationSelectionnee));
        }

        private void LoadBlocks()
        {
            Blocks = new ObservableCollection<Block>(_blockDAL.GetAllBlocks());
            OnPropertyChanged(nameof(Blocks));
        }

        // ===============================
        // Gestion des trains (CRUD)
        // ===============================

        private void AddTrain()
        {
            var stations = _stationDAL.GetAllStations().ToList();

            if (_dialogService.ShowTrainDialog(stations, out var train))
            {
                if (train.Station != null)
                {
                    var trainsInStation = Trains.Count(t =>
                        t.Station != null && t.Station.Id == train.Station.Id);

                    if (trainsInStation >= train.Station.CapaciteMaxTrains)
                    {
                        _dialogService.ShowMessage(
                            $"La capacité maximale de la station '{train.Station.Nom}' est atteinte.",
                            "Erreur de capacité");
                        return;
                    }
                }

                _trainDAL.AddTrain(train);
                Trains.Add(train);

                _dialogService.ShowMessage(
                    $"Train '{train.Nom}' ajouté avec succès!",
                    "Ajout de Train");

                OnPropertyChanged(nameof(Trains));
                OnPropertyChanged(nameof(TrainsEnMouvement));
            }
        }

        private void DeleteTrain()
        {
            var stations = _stationDAL.GetAllStations().ToList();

            if (_dialogService.ShowDeleteTrainDialog(stations, out var trainASupprimer))
            {
                _trainDAL.DeleteTrain(trainASupprimer.Id);
                Trains.Remove(trainASupprimer);

                _dialogService.ShowMessage(
                    $"Train '{trainASupprimer.Nom}' supprimé avec succès!",
                    "Suppression de Train");

                OnPropertyChanged(nameof(Trains));
                OnPropertyChanged(nameof(TrainsEnMouvement));
            }
        }

        // ===============================
        // Règles de sécurité pour les itinéraires
        // ===============================

        private bool RespecteReglesSecurite(Itineraire itineraire, Train train)
        {
            var blocksItineraireIds = itineraire.Etapes
                .Where(e => e.BlockId != null)
                .Select(e => e.BlockId!.Value)
                .Distinct()
                .ToList();

            // Si aucune étape n'est associée à un block, on refuse
            if (!blocksItineraireIds.Any())
                return false;

            var tousLesBlocks = _blockDAL.GetAllBlocks().ToList();
            var dictBlocks = tousLesBlocks.ToDictionary(b => b.Id, b => b);

            // 1) Interdiction de partager un block avec un autre train en transit
            var autresTrainsEnTransit = Trains
                .Where(t => t.Id != train.Id &&
                            t.Etat == EtatTrain.EnTransit &&
                            t.BlockId != null)
                .ToList();

            foreach (var autre in autresTrainsEnTransit)
            {
                if (autre.BlockId == null) continue;

                if (blocksItineraireIds.Contains(autre.BlockId.Value))
                {
                    return false;
                }
            }

            // 2) Distance minimale entre blocks (approximation de "2 blocks de distance")
            const double distanceMinKm = 2.0;

            var blocksItineraire = blocksItineraireIds
                .Where(id => dictBlocks.ContainsKey(id))
                .Select(id => dictBlocks[id])
                .ToList();

            var blocksOccupesAutresTrains = autresTrainsEnTransit
                .Where(t => t.BlockId != null && dictBlocks.ContainsKey(t.BlockId.Value))
                .Select(t => dictBlocks[t.BlockId!.Value])
                .ToList();

            foreach (var blockItineraire in blocksItineraire)
            {
                var centerItin = GetCenter(blockItineraire);

                foreach (var blockAutre in blocksOccupesAutresTrains)
                {
                    var centerAutre = GetCenter(blockAutre);
                    var dKm = DistanceKm(centerItin, centerAutre);

                    if (dKm < distanceMinKm)
                        return false;
                }
            }

            return true;
        }

        // ===============================
        // Association des blocks aux étapes
        // ===============================

        private void AssocierBlocksAuxEtapes(Itineraire itineraire, List<PointArret> arretsSelectionnes)
        {
            var etapes = itineraire.Etapes
                .OrderBy(e => e.Ordre)
                .ToList();

            var blocks = _blockDAL.GetAllBlocks().ToList();

            // On associe chaque étape i au segment entre arret[i] et arret[i+1]
            for (int i = 0; i < etapes.Count - 1 && i < arretsSelectionnes.Count - 1; i++)
            {
                var etape = etapes[i];

                var departArret = arretsSelectionnes[i];
                var arriveeArret = arretsSelectionnes[i + 1];

                var posDepart = new PointLatLng(departArret.Latitude, departArret.Longitude);
                var posArrivee = new PointLatLng(arriveeArret.Latitude, arriveeArret.Longitude);

                Block? meilleurBlock = null;
                double meilleurScore = double.MaxValue;

                foreach (var block in blocks)
                {
                    var start = new PointLatLng(block.LatitudeDepart, block.LongitudeDepart);
                    var end = new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee);

                    // Score = distance départ + distance arrivée (on veut le plus petit)
                    var dStart = DistanceKm(posDepart, start);
                    var dEnd = DistanceKm(posArrivee, end);
                    var score = dStart + dEnd;

                    if (score < meilleurScore)
                    {
                        meilleurScore = score;
                        meilleurBlock = block;
                    }
                }

                if (meilleurBlock != null)
                {
                    etape.BlockId = meilleurBlock.Id;
                }
            }

            // ⚠️ On NE TOUCHE PAS à la dernière étape (arrivée finale).
            // Elle représente un arrêt, pas un segment de déplacement.
        }



        // ===============================
        // Planification d’itinéraire
        // ===============================

        private void PlanifierItineraire()
        {
            var trainsList = _trainDAL.GetAllTrains()
                .Where(t => t.Etat != EtatTrain.EnTransit && t.Etat != EtatTrain.HorsService)
                .ToList();
            if (!trainsList.Any())
            {
                _dialogService.ShowMessage(
                    "Aucun train disponible pour la planification d'itinéraire (tous sont en transit ou hors service).",
                    "Planification impossible");
                return;
            }
            var pointsArret = _pointArretDAL.GetAllPointArrets().ToList();

            if (_dialogService.ShowPlanifierItineraireDialog(
                    trainsList,
                    pointsArret,
                    out var train,
                    out var arretsSelectionnes))
            {
                // 🔒 Règle 1 : on ne planifie pas un train en transit
                if (train.Etat == EtatTrain.EnTransit)
                {
                    _dialogService.ShowMessage(
                        $"Le train '{train.Nom}' est actuellement en transit. " +
                        "Vous ne pouvez pas planifier un nouvel itinéraire tant qu'il n'est pas revenu en gare.",
                        "Planification impossible");
                    return;
                }

                // 🔁 Règle 2 : si un itinéraire existe déjà pour ce train, on le supprime
                var itineraireExistant = _itineraireDAL.GetItineraireParTrain(train.Id);
                if (itineraireExistant != null)
                {
                    _itineraireDAL.SupprimerItineraireParTrain(train.Id);
                }

                var maintenant = DateTime.Now;
                const int minutesParSegment = 10;

                var itineraire = new Itineraire
                {
                    Nom = $"Itinéraire {maintenant:yyyyMMdd_HHmm}",
                    DateDepart = maintenant,
                    DateArrivee = maintenant.AddMinutes(
                        minutesParSegment * Math.Max(1, arretsSelectionnes.Count - 1)),
                    TrainId = train.Id,
                    Etapes = arretsSelectionnes
                        .Select((arret, index) => new Etape
                        {
                            Ordre = index,
                            Lieu = arret.Nom,
                            HeureArrivee = maintenant.AddMinutes(index * minutesParSegment),
                            HeureDepart = maintenant.AddMinutes(index * minutesParSegment + 2),
                            TrainId = train.Id
                        })
                        .ToList()
                };

                AssocierBlocksAuxEtapes(itineraire, arretsSelectionnes);

                if (!RespecteReglesSecurite(itineraire, train))
                {
                    _dialogService.ShowMessage(
                        "L'itinéraire planifié viole les règles de sécurité. " +
                        "Veuillez ajuster les arrêts ou choisir un autre train.",
                        "Itinéraire refusé");
                    return;
                }

                _itineraireDAL.PlanifierItineraire(itineraire);

                // Le train passe à l'état Programmé, puis EnTransit au démarrage de la simulation
                train.Etat = EtatTrain.Programme;
                _trainDAL.UpdateTrain(train);
                LoadTrains();

                PreparerItinerairePourCarteEtSimulation(itineraire, train, arretsSelectionnes);

                _dialogService.ShowMessage("Itinéraire planifié avec succès!", "Planification");
            }
        }


        private void PreparerItinerairePourCarteEtSimulation(
            Itineraire itineraire,
            Train train,
            List<PointArret> arretsSelectionnes)
        {
            var etapes = itineraire.Etapes
                .OrderBy(e => e.Ordre)
                .ToList();

            if (etapes.Count < 2 || arretsSelectionnes.Count < 2)
                return;

            // 🔹 Construire la polyline d’itinéraire en suivant les blocks des étapes (segments)
            ItineraireCourantPoints.Clear();

            var etapesTriees = itineraire.Etapes
                .OrderBy(e => e.Ordre)
                .ToList();

            // On ne prend que les étapes qui ont vraiment un segment après elles
            for (int i = 0; i < etapesTriees.Count - 1; i++)
            {
                var etape = etapesTriees[i];

                if (etape.BlockId == null)
                    continue;

                var block = Blocks.FirstOrDefault(b => b.Id == etape.BlockId.Value);
                if (block == null)
                    continue;

                List<PointLatLng> polyline;

                if (RailGeometry.PolylinesParNomBlock.TryGetValue(block.Nom, out var geoPoints) &&
                    geoPoints != null && geoPoints.Count >= 2)
                {
                    polyline = geoPoints;
                }
                else
                {
                    polyline = new List<PointLatLng>
        {
            new PointLatLng(block.LatitudeDepart, block.LongitudeDepart),
            new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee)
        };
                }

                foreach (var p in polyline)
                {
                    if (ItineraireCourantPoints.Count > 0)
                    {
                        var last = ItineraireCourantPoints[ItineraireCourantPoints.Count - 1];
                        if (Math.Abs(last.Lat - p.Lat) < 1e-6 &&
                            Math.Abs(last.Lng - p.Lng) < 1e-6)
                        {
                            continue;
                        }
                    }

                    ItineraireCourantPoints.Add(p);
                }
            }

            OnPropertyChanged(nameof(ItineraireCourantPoints));



            var simState = new SimulationTrainState
            {
                Train = train,
                Itineraire = itineraire,
                Etapes = etapes,
                PointsArret = arretsSelectionnes,
                IndexSegment = 0,
                DepartSegment = DateTime.Now,
                ArriveeSegment = DateTime.Now.AddSeconds(30) 
            };

            var depart = arretsSelectionnes[0];
            simState.PositionCourante = new PointLatLng(depart.Latitude, depart.Longitude);

            train.Etat = EtatTrain.EnTransit;
            _trainDAL.UpdateTrain(train);

            _simulationsActives[train.Id] = simState;

            LoadTrains();

            if (!_simulationTimer.IsEnabled)
                _simulationTimer.Start();
        }

        private int? TrouverStationProche(PointArret arret)
        {
            if (Stations == null || Stations.Count == 0)
                return null;

            var posArret = new PointLatLng(arret.Latitude, arret.Longitude);

            Station? stationProche = null;
            double distanceMin = double.MaxValue;

            foreach (var station in Stations)
            {
                var posStation = new PointLatLng(station.Latitude, station.Longitude);
                var d = DistanceKm(posArret, posStation);

                if (d < distanceMin)
                {
                    distanceMin = d;
                    stationProche = station;
                }
            }

            return stationProche?.Id;
        }

        // ===============================
        // Simulation (timer)
        // ===============================

      private void SimulationTimer_Tick(object? sender, EventArgs e)
{
    if (_simulationsActives.Count == 0)
    {
        _simulationTimer.Stop();
        return;
    }

    var maintenant = DateTime.Now;
    var simulationsTerminees = new List<int>();

    foreach (var kvp in _simulationsActives)
    {
        var trainId = kvp.Key;
        var sim = kvp.Value;

        // Sécurité : il faut au moins 2 étapes et 2 points d'arrêt
        if (sim.Etapes.Count < 2 || sim.PointsArret.Count < 2)
        {
            simulationsTerminees.Add(trainId);
            continue;
        }

        // Gestion du changement de segment (étape)
        if (maintenant >= sim.ArriveeSegment)
        {
            sim.IndexSegment++;

            // Si on a terminé toutes les étapes -> fin de simulation pour ce train
            if (sim.IndexSegment >= sim.PointsArret.Count - 1)
            {
                simulationsTerminees.Add(trainId);

                var dernierArret = sim.PointsArret.Last();
                sim.Train.Etat = EtatTrain.EnGare;
                sim.Train.StationId = TrouverStationProche(dernierArret);
                sim.Train.BlockId = null;

                _trainDAL.UpdateTrain(sim.Train);
                continue;
            }

            // Nouveau segment : on repart pour 30 secondes
            sim.DepartSegment = maintenant;
            sim.ArriveeSegment = maintenant.AddSeconds(30);
        }

        // Progression dans le segment actuel
        var dureeSegment = sim.ArriveeSegment - sim.DepartSegment;
        if (dureeSegment.TotalSeconds <= 0)
            continue;

        var ecoule = maintenant - sim.DepartSegment;
        var ratio = Math.Clamp(ecoule.TotalSeconds / dureeSegment.TotalSeconds, 0.0, 1.0);

        PointLatLng nouvellePosition;

        // 🔹 On cherche d'abord à suivre la polyline du BLOCK de l'étape courante
        var etapeCourante = sim.Etapes[sim.IndexSegment];
        Block? blockCourant = null;

        if (etapeCourante.BlockId != null)
        {
            // On cherche le block correspondant dans ceux chargés en mémoire
            blockCourant = Blocks.FirstOrDefault(b => b.Id == etapeCourante.BlockId.Value);
        }

        if (blockCourant != null &&
            RailGeometry.PolylinesParNomBlock.TryGetValue(blockCourant.Nom, out var polyline) &&
            polyline != null && polyline.Count >= 2)
        {
            // ✅ Le block a une polyline définie : on avance le train le long de cette polyline

            var nbSegmentsPolyline = polyline.Count - 1;    // ex : 5 points => 4 segments
            var tGlobal = ratio * nbSegmentsPolyline;       // ex : ratio 0.5 => milieu de la polyline
            var indexSegmentPolyline = (int)Math.Floor(tGlobal);

            if (indexSegmentPolyline >= nbSegmentsPolyline)
                indexSegmentPolyline = nbSegmentsPolyline - 1;

            var tLocal = tGlobal - indexSegmentPolyline;    // entre 0 et 1 dans ce sous-segment

            var p0 = polyline[indexSegmentPolyline];
            var p1 = polyline[indexSegmentPolyline + 1];

            var lat = p0.Lat + (p1.Lat - p0.Lat) * tLocal;
            var lng = p0.Lng + (p1.Lng - p0.Lng) * tLocal;

            nouvellePosition = new PointLatLng(lat, lng);
        }
        else
        {
            // ⚠️ Fallback : si pas de block / pas de polyline, on garde l'ancienne logique
            var depart = sim.PointsArret[sim.IndexSegment];
            var arrivee = sim.PointsArret[sim.IndexSegment + 1];

            var lat = depart.Latitude + (arrivee.Latitude - depart.Latitude) * ratio;
            var lng = depart.Longitude + (arrivee.Longitude - depart.Longitude) * ratio;

            nouvellePosition = new PointLatLng(lat, lng);
        }

        // Mise à jour de la position du train simulé
        sim.PositionCourante = nouvellePosition;
    }

            // Nettoyer les simulations terminées
            foreach (var id in simulationsTerminees)
            {
                _simulationsActives.Remove(id);
            }

            // Rafraîchir les Trains pour l'UI
            LoadTrains();

            // Mettre à jour les blocks occupés + détection des conflits
            MettreAJourBlocksEtConflits();

            // Notifier la vue que la position des trains dynamiques a changé
            OnPropertyChanged(nameof(SimulationsActives));
        }


        // ===============================
        // Block control + conflits
        // ===============================

        private void MettreAJourBlocksEtConflits()
        {
            if (_simulationsActives.Count == 0)
                return;

            var blocks = _blockDAL.GetAllBlocks().ToList();

            // Libérer les blocks occupés par les trains simulés
            foreach (var sim in _simulationsActives.Values)
            {
                if (sim.Train.BlockId == null)
                    continue;

                var ancienBlock = blocks.FirstOrDefault(b => b.Id == sim.Train.BlockId.Value);
                if (ancienBlock != null)
                {
                    ancienBlock.EstOccupe = false;
                    _blockDAL.UpdateBlock(ancienBlock);
                }

                sim.Train.BlockId = null;
                _trainDAL.UpdateTrain(sim.Train);
            }

            // Assigner un nouveau block à chaque train en fonction de sa position
            foreach (var sim in _simulationsActives.Values)
            {
                var blockLePlusProche = TrouverBlockLePlusProche(sim.PositionCourante, blocks);

                if (blockLePlusProche != null)
                {
                    blockLePlusProche.EstOccupe = true;
                    _blockDAL.UpdateBlock(blockLePlusProche);

                    sim.Train.BlockId = blockLePlusProche.Id;
                    _trainDAL.UpdateTrain(sim.Train);
                }
            }

            LoadBlocks();
            LoadTrains();

            var conflits = GetConflits();
            ConflitsTextuels = new ObservableCollection<string>(
                conflits.Select(c => $"⚠️ {c.TrainA.Nom} et {c.TrainB.Nom} sur {c.BlockConflit.Nom}")
            );

            OnPropertyChanged(nameof(ConflitsDeLaStationSelectionnee));
        }

        // ===============================
        // Utilitaires géométriques
        // ===============================

        private static PointLatLng GetCenter(Block block)
        {
            return new PointLatLng(
                (block.LatitudeDepart + block.LatitudeArrivee) / 2,
                (block.LongitudeDepart + block.LongitudeArrivee) / 2
            );
        }

        private Block? TrouverBlockLePlusProche(PointLatLng position, IList<Block> blocks)
        {
            Block? blockLePlusProche = null;
            double distanceMin = double.MaxValue;

            foreach (var block in blocks)
            {
                var center = GetCenter(block);
                var d = DistanceKm(center, position);

                if (d < distanceMin)
                {
                    distanceMin = d;
                    blockLePlusProche = block;
                }
            }

            return blockLePlusProche;
        }

        private double DistanceKm(PointLatLng a, PointLatLng b)
        {
            const double R = 6371; // Rayon terrestre en km
            var dLat = (b.Lat - a.Lat) * Math.PI / 180;
            var dLon = (b.Lng - a.Lng) * Math.PI / 180;
            var lat1 = a.Lat * Math.PI / 180;
            var lat2 = b.Lat * Math.PI / 180;

            var aCalc = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(aCalc), Math.Sqrt(1 - aCalc));
            return R * c;
        }

        // ===============================
        // Détection des conflits
        // ===============================

        public List<(Train TrainA, Train TrainB, Block BlockConflit)> GetConflits()
        {
            var conflits = new List<(Train, Train, Block)>();

            var trainsEnTransit = Trains
                .Where(t => t.Etat == EtatTrain.EnTransit && t.BlockId != null)
                .ToList();

            var blocksOccupes = Blocks
                .Where(b => b.EstOccupe)
                .ToDictionary(b => b.Id, b => b);

            for (var i = 0; i < trainsEnTransit.Count; i++)
            {
                var trainA = trainsEnTransit[i];
                var blockA = blocksOccupes.GetValueOrDefault(trainA.BlockId!.Value);
                if (blockA == null) continue;

                for (var j = i + 1; j < trainsEnTransit.Count; j++)
                {
                    var trainB = trainsEnTransit[j];
                    var blockB = blocksOccupes.GetValueOrDefault(trainB.BlockId!.Value);
                    if (blockB == null) continue;

                    // Cas 1 : même block occupé par deux trains
                    if (trainA.BlockId == trainB.BlockId)
                    {
                        conflits.Add((trainA, trainB, blockA));
                        continue;
                    }

                    // Cas 2 : blocks différents mais trop proches
                    var centerA = GetCenter(blockA);
                    var centerB = GetCenter(blockB);

                    var distanceKm = DistanceKm(centerA, centerB);

                    if (distanceKm < 1.0)
                    {
                        conflits.Add((trainA, trainB, blockA));
                    }
                }
            }

            return conflits;
        }
    }
}
