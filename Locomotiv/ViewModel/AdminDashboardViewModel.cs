using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using GMap.NET;
using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.ViewModel
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        // ===============================
        // Champs privés
        // ===============================

        private readonly ITrainDAL _trainDAL;
        private readonly IStationDAL _stationDAL;
        private readonly IDialogService _dialogService;
        private readonly IBlockDAL _blockDAL;
        private readonly IPointArretDAL _pointArretDAL;
        private readonly IItineraireDAL _itineraireDAL;
        private readonly DispatcherTimer _simulationTimer;

        // Tous les trains en simulation, indexés par Train.Id
        private readonly Dictionary<int, SimulationTrainState> _simulationsActives = new();

        // ===============================
        // Collections exposées
        // ===============================

        public ObservableCollection<Block> Blocks { get; private set; } = new();
        public ObservableCollection<Train> Trains { get; private set; } = new();
        public ObservableCollection<Station> Stations { get; private set; } = new();
        public ObservableCollection<PointArret> PointsInteret { get; private set; } = new();

        /// <summary>Points utilisés pour tracer la polyline d’itinéraire sur la carte.</summary>
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

        /// <summary>Positions des trains en simulation (pour le code-behind / markers).</summary>
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
                    t.Station?.Id == StationSelectionnee?.Id && t.Etat == EtatTrain.EnGare
                )
            );

        public ObservableCollection<Train> DepartsDeLaStationSelectionnee =>
            new ObservableCollection<Train>(
                Trains.Where(t =>
                    t.Station?.Id == StationSelectionnee?.Id && t.Etat == EtatTrain.EnAttente
                )
            );

        public ObservableCollection<string> ConflitsDeLaStationSelectionnee =>
            new ObservableCollection<string>(
                GetConflits()
                    .Where(c =>
                        c.TrainA.Station?.Id == StationSelectionnee?.Id
                        || c.TrainB.Station?.Id == StationSelectionnee?.Id
                    )
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

        /// <summary>Utilisé par le code-behind (markers).</summary>
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

        /// <summary>Utilisé par le XAML.</summary>
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
            IItineraireDAL itineraireDAL
        )
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

            _simulationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _simulationTimer.Tick += SimulationTimer_Tick;
        }

        // ===============================
        // Chargement des données
        // ===============================

        public void LoadStations()
        {
            Stations = new ObservableCollection<Station>(_stationDAL.GetAllStations());
            OnPropertyChanged(nameof(Stations));
        }

        public void LoadPointsInteret()
        {
            PointsInteret = new ObservableCollection<PointArret>(
                _pointArretDAL.GetAllPointArrets()
            );
            OnPropertyChanged(nameof(PointsInteret));
        }

        public void LoadTrains()
        {
            Trains = new ObservableCollection<Train>(_trainDAL.GetAllTrains());
            OnPropertyChanged(nameof(Trains));
            OnPropertyChanged(nameof(TrainsEnMouvement));
            OnPropertyChanged(nameof(TrainsDeLaStationSelectionnee));
            OnPropertyChanged(nameof(ArriveesDeLaStationSelectionnee));
            OnPropertyChanged(nameof(DepartsDeLaStationSelectionnee));
        }

        public void LoadBlocks()
        {
            Blocks = new ObservableCollection<Block>(_blockDAL.GetAllBlocks());
            OnPropertyChanged(nameof(Blocks));
        }

        // ===============================
        // Gestion des trains (CRUD)
        // ===============================

        public void AddTrain()
        {
            var stationsDisponibles = GetStationsAvecCapaciteDisponible().ToList();

            if (!stationsDisponibles.Any())
            {
                _dialogService.ShowMessage(
                    "Aucune station n’a de capacité disponible pour accueillir un nouveau train.",
                    "Capacité atteinte"
                );
                return;
            }

            if (_dialogService.ShowTrainDialog(stationsDisponibles, out var train))
            {
                if (!StationAEncoreDeLaPlace(train.Station))
                {
                    _dialogService.ShowMessage(
                        $"La capacité maximale de la station '{train.Station?.Nom}' est atteinte.",
                        "Erreur de capacité"
                    );
                    return;
                }

                _trainDAL.AddTrain(train);
                Trains.Add(train);

                _dialogService.ShowMessage(
                    $"Train '{train.Nom}' ajouté avec succès!",
                    "Ajout de Train"
                );

                OnTrainsChanged();
            }
        }

        public IEnumerable<Station> GetStationsAvecCapaciteDisponible()
        {
            var allStations = _stationDAL.GetAllStations();
            return allStations.Where(station =>
                Trains.Count(t => t.Station != null && t.Station.Id == station.Id)
                < station.CapaciteMaxTrains
            );
        }

        public bool StationAEncoreDeLaPlace(Station? station)
        {
            if (station == null)
                return true;

            var trainsDansStation = Trains.Count(t =>
                t.Station != null && t.Station.Id == station.Id
            );
            return trainsDansStation < station.CapaciteMaxTrains;
        }

        public void DeleteTrain()
        {
            var stations = _stationDAL.GetAllStations().ToList();

            if (_dialogService.ShowDeleteTrainDialog(stations, out var trainASupprimer))
            {
                _trainDAL.DeleteTrain(trainASupprimer.Id);
                Trains.Remove(trainASupprimer);

                _dialogService.ShowMessage(
                    $"Train '{trainASupprimer.Nom}' supprimé avec succès!",
                    "Suppression de Train"
                );

                OnTrainsChanged();
            }
        }

        public void OnTrainsChanged()
        {
            OnPropertyChanged(nameof(Trains));
            OnPropertyChanged(nameof(TrainsEnMouvement));
            OnPropertyChanged(nameof(TrainsDeLaStationSelectionnee));
            OnPropertyChanged(nameof(ArriveesDeLaStationSelectionnee));
            OnPropertyChanged(nameof(DepartsDeLaStationSelectionnee));
        }

        // ===============================
        // Règles de sécurité pour les itinéraires
        // ===============================

        private bool RespecteReglesSecurite(Itineraire itineraire, Train train)
        {
            var blocksItineraireIds = MathUtils.GetBlocksIdsForItinerary(itineraire);

            if (!blocksItineraireIds.Any())
                return false;

            var tousLesBlocks = _blockDAL.GetAllBlocks().ToList();
            var dictBlocks = tousLesBlocks.ToDictionary(b => b.Id, b => b);

            var autresTrainsEnTransit = Trains
                .Where(t => t.Id != train.Id && t.Etat == EtatTrain.EnTransit && t.BlockId != null)
                .ToList();

            if (
                MathUtils.PartageUnBlockAvecUnAutreTrain(blocksItineraireIds, autresTrainsEnTransit)
            )
                return false;

            if (!RespecteDistanceMinimale(blocksItineraireIds, dictBlocks, autresTrainsEnTransit))
                return false;

            return true;
        }

        private bool RespecteDistanceMinimale(
            List<int> blocksItineraireIds,
            Dictionary<int, Block> dictBlocks,
            List<Train> autresTrainsEnTransit
        )
        {
            const double distanceMinKm = 2.0;

            var blocksItineraire = blocksItineraireIds
                .Where(dictBlocks.ContainsKey)
                .Select(id => dictBlocks[id])
                .ToList();

            var blocksOccupesAutresTrains = autresTrainsEnTransit
                .Where(t => t.BlockId != null && dictBlocks.ContainsKey(t.BlockId.Value))
                .Select(t => dictBlocks[t.BlockId!.Value])
                .ToList();

            foreach (var blockItineraire in blocksItineraire)
            {
                var centerItin = MathUtils.GetCenter(blockItineraire);

                foreach (var blockAutre in blocksOccupesAutresTrains)
                {
                    var centerAutre = MathUtils.GetCenter(blockAutre);
                    var dKm = MathUtils.DistanceKm(centerItin, centerAutre);

                    if (dKm < distanceMinKm)
                        return false;
                }
            }

            return true;
        }

        // ===============================
        // Association des blocks aux étapes
        // ===============================

        private void AssocierBlocksAuxEtapes(
            Itineraire itineraire,
            List<PointArret> arretsSelectionnes
        )
        {
            var etapes = itineraire.Etapes.OrderBy(e => e.Ordre).ToList();
            var blocks = _blockDAL.GetAllBlocks().ToList();

            for (int i = 0; i < etapes.Count - 1 && i < arretsSelectionnes.Count - 1; i++)
            {
                var etape = etapes[i];

                var departArret = arretsSelectionnes[i];
                var arriveeArret = arretsSelectionnes[i + 1];

                var posDepart = new PointLatLng(departArret.Latitude, departArret.Longitude);
                var posArrivee = new PointLatLng(arriveeArret.Latitude, arriveeArret.Longitude);

                var meilleurBlock = TrouverBlockPourSegment(posDepart, posArrivee, blocks);

                if (meilleurBlock != null)
                    etape.BlockId = meilleurBlock.Id;
            }
        }

        private Block? TrouverBlockPourSegment(
            PointLatLng posDepart,
            PointLatLng posArrivee,
            IList<Block> blocks
        )
        {
            Block? meilleurBlock = null;
            double meilleurScore = double.MaxValue;

            foreach (var block in blocks)
            {
                var start = new PointLatLng(block.LatitudeDepart, block.LongitudeDepart);
                var end = new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee);

                var dStart = MathUtils.DistanceKm(posDepart, start);
                var dEnd = MathUtils.DistanceKm(posArrivee, end);
                var score = dStart + dEnd;

                if (score < meilleurScore)
                {
                    meilleurScore = score;
                    meilleurBlock = block;
                }
            }

            return meilleurBlock;
        }

        // ===============================
        // Planification d’itinéraire
        // ===============================

        public void PlanifierItineraire()
        {
            var trainsDisponibles = GetTrainsDisponiblesPourPlanification().ToList();

            if (!trainsDisponibles.Any())
            {
                _dialogService.ShowMessage(
                    "Aucun train disponible pour la planification d'itinéraire (tous sont en transit ou hors service).",
                    "Planification impossible"
                );
                return;
            }

            var pointsArret = _pointArretDAL.GetAllPointArrets().ToList();

            if (
                _dialogService.ShowPlanifierItineraireDialog(
                    trainsDisponibles,
                    pointsArret,
                    out var train,
                    out var arretsSelectionnes,
                    out var dateDepart,
                    out var dateArrivee
                )
            )
            {
                if (train == null || !PeutPlanifierPourTrain(train))
                    return;

                SupprimerItineraireExistant(train.Id);

                var arretsEtendus = RailRoutes.ExpandItinerary(arretsSelectionnes, pointsArret);

                if (arretsEtendus.Count < 2)
                    arretsEtendus = arretsSelectionnes;

                var dureeTotale = dateArrivee - dateDepart;
                var nbSegments = Math.Max(1, arretsEtendus.Count - 1);
                var dureeParSegment = TimeSpan.FromTicks(dureeTotale.Ticks / nbSegments);

                var itineraire = new Itineraire
                {
                    Nom = $"Itinéraire {dateDepart:yyyyMMdd_HHmm}",
                    DateDepart = dateDepart,
                    DateArrivee = dateArrivee,
                    TrainId = train.Id,
                    Etapes = arretsEtendus
                        .Select(
                            (arret, index) =>
                                new Etape
                                {
                                    Ordre = index,
                                    Lieu = arret.Nom,
                                    HeureArrivee = dateDepart + dureeParSegment * index,
                                    HeureDepart =
                                        dateDepart
                                        + dureeParSegment * index
                                        + TimeSpan.FromMinutes(2),
                                    TrainId = train.Id,
                                }
                        )
                        .ToList(),
                };

                // ⚠️ Très important : on associe les blocks sur les arrêts ÉTENDUS
                AssocierBlocksAuxEtapes(itineraire, arretsEtendus);

                if (!RespecteReglesSecurite(itineraire, train))
                {
                    _dialogService.ShowMessage(
                        "L'itinéraire planifié viole les règles de sécurité. "
                            + "Veuillez ajuster les arrêts ou choisir un autre train.",
                        "Itinéraire refusé"
                    );
                    return;
                }

                _itineraireDAL.PlanifierItineraire(itineraire);

                train.Etat = EtatTrain.Programme;
                _trainDAL.UpdateTrain(train);
                LoadTrains();

                // On simule et on dessine aussi avec la liste ÉTENDUE
                PreparerItinerairePourCarteEtSimulation(itineraire, train, arretsEtendus);

                _dialogService.ShowMessage("Itinéraire planifié avec succès!", "Planification");
            }
        }

        public IEnumerable<Train> GetTrainsDisponiblesPourPlanification()
        {
            return _trainDAL
                .GetAllTrains()
                .Where(t => t.Etat != EtatTrain.EnTransit && t.Etat != EtatTrain.HorsService);
        }

        public bool PeutPlanifierPourTrain(Train train)
        {
            if (train.Etat == EtatTrain.EnTransit)
            {
                _dialogService.ShowMessage(
                    $"Le train '{train.Nom}' est actuellement en transit. "
                        + "Vous ne pouvez pas planifier un nouvel itinéraire tant qu'il n'est pas revenu en gare.",
                    "Planification impossible"
                );
                return false;
            }
            return true;
        }

        public void SupprimerItineraireExistant(int trainId)
        {
            var itineraireExistant = _itineraireDAL.GetItineraireParTrain(trainId);
            if (itineraireExistant != null)
            {
                _itineraireDAL.SupprimerItineraireParTrain(trainId);
            }
        }

        public void DemarrerSimulation(int trainId)
        {
            if (_simulationsActives.TryGetValue(trainId, out var sim))
            {
                if (DateTime.Now < sim.DateDepartPlanifie)
                {
                    var delai = sim.DateDepartPlanifie - DateTime.Now;

                    sim.Train.Etat = EtatTrain.Programme;
                    _trainDAL.UpdateTrain(sim.Train);

                    Task.Delay(delai)
                        .ContinueWith(_ =>
                        {
                            // Quand l'heure arrive → démarrage réel
                            sim.DateDebutSimulation = DateTime.Now;
                            sim.Train.Etat = EtatTrain.EnTransit;
                            _trainDAL.UpdateTrain(sim.Train);

                            if (!_simulationTimer.IsEnabled)
                                _simulationTimer.Start();

                            LoadTrains();
                            MettreAJourBlocksEtConflits();
                            OnPropertyChanged(nameof(SimulationsActives));
                        });
                }
                else
                {
                    sim.DateDebutSimulation = DateTime.Now;
                    sim.Train.Etat = EtatTrain.EnTransit;
                    _trainDAL.UpdateTrain(sim.Train);

                    if (!_simulationTimer.IsEnabled)
                        _simulationTimer.Start();

                    LoadTrains();
                    MettreAJourBlocksEtConflits();
                    OnPropertyChanged(nameof(SimulationsActives));
                }
            }
        }

        public void PreparerItinerairePourCarteEtSimulation(
            Itineraire itineraire,
            Train train,
            List<PointArret> arretsSelectionnes
        )
        {
            var etapes = itineraire.Etapes.OrderBy(e => e.Ordre).ToList();

            if (etapes.Count < 2 || arretsSelectionnes.Count < 2)
                return;

            ConstruirePolylineItineraire(itineraire);

            var simState = new SimulationTrainState
            {
                Train = train,
                Itineraire = itineraire,
                Etapes = etapes,
                PointsArret = arretsSelectionnes,
                IndexSegment = 0,
                DepartSegment = itineraire.DateDepart,
                ArriveeSegment = itineraire.DateArrivee,
                PositionCourante = new PointLatLng(0, 0),
                DateDepartPlanifie = itineraire.DateDepart,
                DateDebutSimulation = DateTime.Now,
            };

            var depart = arretsSelectionnes[0];
            simState.PositionCourante = new PointLatLng(depart.Latitude, depart.Longitude);

            _simulationsActives[train.Id] = simState;

            if (itineraire.DateDepart > DateTime.Now)
            {
                train.Etat = EtatTrain.Programme;
                _trainDAL.UpdateTrain(train);

                var delai = itineraire.DateDepart - DateTime.Now;
                Task.Delay(delai).ContinueWith(_ => DemarrerSimulation(train.Id));
            }
            else
            {
                train.Etat = EtatTrain.EnTransit;
                _trainDAL.UpdateTrain(train);

                if (!_simulationTimer.IsEnabled)
                    _simulationTimer.Start();
            }

            LoadTrains();
        }

        public void ConstruirePolylineItineraire(Itineraire itineraire)
        {
            ItineraireCourantPoints.Clear();

            var etapesTriees = itineraire.Etapes.OrderBy(e => e.Ordre).ToList();

            for (int i = 0; i < etapesTriees.Count - 1; i++)
            {
                var etape = etapesTriees[i];

                if (etape.BlockId == null)
                    continue;

                var block = Blocks.FirstOrDefault(b => b.Id == etape.BlockId.Value);
                if (block == null)
                    continue;

                foreach (var p in MathUtils.GetPolylinePointsForBlock(block))
                {
                    if (ItineraireCourantPoints.Count > 0)
                    {
                        var last = ItineraireCourantPoints[^1];
                        if (Math.Abs(last.Lat - p.Lat) < 1e-6 && Math.Abs(last.Lng - p.Lng) < 1e-6)
                            continue;
                    }
                    ItineraireCourantPoints.Add(p);
                }
            }

            OnPropertyChanged(nameof(ItineraireCourantPoints));
        }

        public int? TrouverStationProche(PointArret arret)
        {
            if (Stations == null || Stations.Count == 0)
                return null;

            var posArret = new PointLatLng(arret.Latitude, arret.Longitude);

            Station? stationProche = null;
            double distanceMin = double.MaxValue;

            foreach (var station in Stations)
            {
                var posStation = new PointLatLng(station.Latitude, station.Longitude);
                var d = MathUtils.DistanceKm(posArret, posStation);

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

        public void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            if (_simulationsActives.Count == 0)
            {
                _simulationTimer.Stop();
                return;
            }

            var simulationsTerminees = new List<int>();

            foreach (var (trainId, sim) in _simulationsActives.ToList())
            {
                if (DateTime.Now < sim.DateDepartPlanifie)
                {
                    sim.Train.Etat = EtatTrain.Programme;
                    _trainDAL.UpdateTrain(sim.Train);
                    continue;
                }

                var tempsEcoule = DateTime.Now - sim.DateDebutSimulation;
                var maintenantSimule = sim.DateDepartPlanifie + tempsEcoule;

                if (!MettreAJourSimulationTrain(sim, maintenantSimule))
                {
                    simulationsTerminees.Add(trainId);
                }
                else
                {
                    if (sim.Train.Etat != EtatTrain.EnTransit)
                    {
                        sim.Train.Etat = EtatTrain.EnTransit;
                        _trainDAL.UpdateTrain(sim.Train);
                    }
                }
            }

            foreach (var id in simulationsTerminees)
            {
                _simulationsActives.Remove(id);
            }

            LoadTrains();
            MettreAJourBlocksEtConflits();
            OnPropertyChanged(nameof(SimulationsActives));
        }

        /// <summary>
        /// Met à jour la position d'un train simulé.
        /// Retourne false si la simulation est terminée pour ce train.
        /// </summary>
        public bool MettreAJourSimulationTrain(SimulationTrainState sim, DateTime maintenant)
        {
            if (sim.Etapes.Count < 2 || sim.PointsArret.Count < 2)
                return false;

            if (maintenant >= sim.ArriveeSegment)
            {
                sim.IndexSegment++;

                if (sim.IndexSegment >= sim.PointsArret.Count - 1)
                {
                    // Fin de l’itinéraire pour ce train
                    var dernierArret = sim.PointsArret.Last();
                    sim.Train.Etat = EtatTrain.EnGare;
                    sim.Train.StationId = TrouverStationProche(dernierArret);
                    sim.Train.BlockId = null;

                    _trainDAL.UpdateTrain(sim.Train);
                    return false;
                }

                sim.DepartSegment = maintenant;
                sim.ArriveeSegment = maintenant.AddSeconds(30);
            }

            var dureeSegment = sim.ArriveeSegment - sim.DepartSegment;
            if (dureeSegment.TotalSeconds <= 0)
                return true;

            var ecoule = maintenant - sim.DepartSegment;
            var ratio = Math.Clamp(ecoule.TotalSeconds / dureeSegment.TotalSeconds, 0.0, 1.0);

            sim.PositionCourante = CalculerPositionTrainSurSegment(sim, ratio);

            return true;
        }

        public PointLatLng CalculerPositionTrainSurSegment(SimulationTrainState sim, double ratio)
        {
            var etapeCourante = sim.Etapes[sim.IndexSegment];
            Block? blockCourant =
                etapeCourante.BlockId != null
                    ? Blocks.FirstOrDefault(b => b.Id == etapeCourante.BlockId.Value)
                    : null;

            if (
                blockCourant != null
                && RailGeometry.PolylinesParNomBlock.TryGetValue(blockCourant.Nom, out var polyline)
                && polyline != null
                && polyline.Count >= 2
            )
            {
                var nbSegmentsPolyline = polyline.Count - 1;
                var tGlobal = ratio * nbSegmentsPolyline;
                var indexSegmentPolyline = (int)Math.Floor(tGlobal);

                if (indexSegmentPolyline >= nbSegmentsPolyline)
                    indexSegmentPolyline = nbSegmentsPolyline - 1;

                var tLocal = tGlobal - indexSegmentPolyline;

                var p0 = polyline[indexSegmentPolyline];
                var p1 = polyline[indexSegmentPolyline + 1];

                var lat = p0.Lat + (p1.Lat - p0.Lat) * tLocal;
                var lng = p0.Lng + (p1.Lng - p0.Lng) * tLocal;

                return new PointLatLng(lat, lng);
            }

            var depart = sim.PointsArret[sim.IndexSegment];
            var arrivee = sim.PointsArret[sim.IndexSegment + 1];

            var latFallback = depart.Latitude + (arrivee.Latitude - depart.Latitude) * ratio;
            var lngFallback = depart.Longitude + (arrivee.Longitude - depart.Longitude) * ratio;

            return new PointLatLng(latFallback, lngFallback);
        }

        // ===============================
        // Block control + conflits
        // ===============================

        public void MettreAJourBlocksEtConflits()
        {
            if (_simulationsActives.Count == 0)
                return;

            var blocks = _blockDAL.GetAllBlocks().ToList();

            LibererBlocksOccupes(blocks);
            AssignerBlocksAuxTrains(blocks);

            LoadBlocks();
            LoadTrains();

            var conflits = GetConflits();
            ConflitsTextuels = new ObservableCollection<string>(
                conflits.Select(c =>
                    $"⚠️ {c.TrainA.Nom} et {c.TrainB.Nom} sur {c.BlockConflit.Nom}"
                )
            );

            OnPropertyChanged(nameof(ConflitsDeLaStationSelectionnee));
        }

        public void LibererBlocksOccupes(List<Block> blocks)
        {
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
        }

        public void AssignerBlocksAuxTrains(List<Block> blocks)
        {
            foreach (var sim in _simulationsActives.Values)
            {
                var blockLePlusProche = TrouverBlockLePlusProche(sim.PositionCourante, blocks);

                if (blockLePlusProche == null)
                    continue;

                blockLePlusProche.EstOccupe = true;
                _blockDAL.UpdateBlock(blockLePlusProche);

                sim.Train.BlockId = blockLePlusProche.Id;
                _trainDAL.UpdateTrain(sim.Train);
            }
        }

        public Block? TrouverBlockLePlusProche(PointLatLng position, IList<Block> blocks)
        {
            Block? blockLePlusProche = null;
            double distanceMin = double.MaxValue;

            foreach (var block in blocks)
            {
                var center = MathUtils.GetCenter(block);
                var d = MathUtils.DistanceKm(center, position);

                if (d < distanceMin)
                {
                    distanceMin = d;
                    blockLePlusProche = block;
                }
            }

            return blockLePlusProche;
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

            var blocksOccupes = Blocks.Where(b => b.EstOccupe).ToDictionary(b => b.Id, b => b);

            for (var i = 0; i < trainsEnTransit.Count; i++)
            {
                var trainA = trainsEnTransit[i];
                var blockA = blocksOccupes.GetValueOrDefault(trainA.BlockId!.Value);
                if (blockA == null)
                    continue;

                for (var j = i + 1; j < trainsEnTransit.Count; j++)
                {
                    var trainB = trainsEnTransit[j];
                    var blockB = blocksOccupes.GetValueOrDefault(trainB.BlockId!.Value);
                    if (blockB == null)
                        continue;

                    // Cas 1 : même block
                    if (trainA.BlockId == trainB.BlockId)
                    {
                        conflits.Add((trainA, trainB, blockA));
                        continue;
                    }

                    // Cas 2 : blocks différents mais trop proches
                    var centerA = MathUtils.GetCenter(blockA);
                    var centerB = MathUtils.GetCenter(blockB);

                    var distanceKm = MathUtils.DistanceKm(centerA, centerB);

                    if (distanceKm < 1.0)
                        conflits.Add((trainA, trainB, blockA));
                }
            }

            return conflits;
        }
    }
}
