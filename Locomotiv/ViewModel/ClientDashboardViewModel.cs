using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Locomotiv.Model; // pour User
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces; // pour IUserSessionService

namespace Locomotiv.ViewModel
{
    public class PassengerRoute : BaseViewModel
    {
        private readonly Itineraire _itineraire;
        private readonly IItineraireService _itineraireService;
        private static readonly Random _random = new Random();

        public int ItineraireId => _itineraire.Id;

        public string TrainNumber
        {
            get
            {
                if (_itineraire.Train != null)
                {
                    return _itineraire.Train.Nom;
                }
                return "Non disponible";
            }
        }

        public DateTime DepartureTime => _itineraire.DateDepart;
        public DateTime ArrivalTime => _itineraire.DateArrivee;

        public string IntermediateStations
        {
            get
            {
                try
                {
                    if (_itineraire.Etapes == null || !_itineraire.Etapes.Any())
                        return "Aucune";

                    var stations = _itineraire.Etapes
                        .OrderBy(e => e.Ordre)
                        .Skip(1)
                        .Take(Math.Max(0, _itineraire.Etapes.Count - 2))
                        .Select(e => e.Lieu);

                    if (stations.Any())
                        return string.Join(", ", stations);
                    else
                        return "Direct";
                }
                catch (Exception ex)
                {
                    return $"{ex.Message}";
                }
            }
        }
        public string FinalDestination
        {
            get
            {
                try
                {
                    if (_itineraire.Etapes == null || !_itineraire.Etapes.Any())
                        return "Non disponible";

                    Etape? lastStation = _itineraire.Etapes
                        .OrderByDescending(e => e.Ordre)
                        .FirstOrDefault();

                    if (lastStation != null)
                        return lastStation.Lieu;

                    return "Non disponible";

                }
                catch (Exception ex)
                {
                    return $"{ex.Message}";
                }
            }
        }

        private int _availableSeats;
        public int AvailableSeats
        {
            get => _availableSeats;
            set
            {
                _availableSeats = value;
                OnPropertyChanged(nameof(AvailableSeats));
                OnPropertyChanged(nameof(CanReserve));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string Status
        {
            get
            {
                if (DepartureTime <= DateTime.Now)
                    return "Départ effectué";

                if (AvailableSeats == 0)
                    return "Complet";

                if (AvailableSeats == 1)
                    return "Dernières places";

                if (AvailableSeats <= 5)
                    return "Quelques places disponibles";

                return "Disponible";
            }
        }
        public string StatusMessage
        {
            get
            {
                if (AvailableSeats == 0)
                    return "Train complet";

                if (AvailableSeats <= 5)
                    return $"Il ne reste que {AvailableSeats} places !";

                return $"{AvailableSeats} places disponibles";
            }
        }

        public decimal Price { get; set; } = _random.Next(10 , 80);

        public bool CanReserve
        {
            get
            {
                if (AvailableSeats > 0 && DepartureTime > DateTime.Now)
                    return true;

                return false;
            }
        }
        public PassengerRoute( Itineraire itineraire, IItineraireService itineraireService)
        {
            if (itineraire == null)
                throw new ArgumentNullException(nameof(itineraire));
            if (itineraireService == null)
                throw new ArgumentNullException(nameof(itineraireService));

            _itineraire = itineraire;
            _itineraireService = itineraireService;

            RefreshAvailableSeats();
        }
        public void RefreshAvailableSeats()
        {
            try
            {
                AvailableSeats = _itineraireService.GetPlacesDisponibles(ItineraireId);
            }
            catch
            {
                AvailableSeats = 0;
            }
        }

    }

    public class ClientDashboardViewModel : BaseViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IItineraireService _itineraireService;

        public User? ConnectedUser => _userSessionService.ConnectedUser;

        public ObservableCollection<PassengerRoute> TrainRoutes { get; } = new();
        public ICollectionView RoutesView { get; }

        private PassengerRoute _selectedRoute;
        public PassengerRoute SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                _selectedRoute = value;
                OnPropertyChanged(nameof(SelectedRoute));

                ClearErrors("Reservation");
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Filtres
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                
            }
        }

        private string _departureTimeFilter;
        public string DepartureTimeFilter
        {
            get => _departureTimeFilter;
            set
            {
                _departureTimeFilter = value;
                OnPropertyChanged(nameof(DepartureTimeFilter));
                if (RoutesView != null)
                {
                   RoutesView.Refresh();
                }          
            }
        }

        private string _destinationFilter;
        public string DestinationFilter
        {
            get => _destinationFilter;
            set
            {
                _destinationFilter = value;
                OnPropertyChanged(nameof(DestinationFilter));
            }
        }

        // Commandes
        public ICommand SearchCommand {  get; }
        public ICommand ReserveCommand { get; }


        public ClientDashboardViewModel(IUserSessionService userSessionService, IItineraireService itineraireService)
        {
            if (userSessionService == null)
                throw new ArgumentNullException(nameof(userSessionService));

            if (itineraireService == null)
                throw new ArgumentNullException(nameof(itineraireService));


            _userSessionService = userSessionService;
            _itineraireService = itineraireService;

            SearchCommand = new RelayCommand(LoadRoutes);
            ReserveCommand = new RelayCommand(ReserveSelected, CanReserveExecute);

            _userSessionService.PropertyChanged += OnUserSessionChanged;
            LoadRoutes();

            RoutesView = CollectionViewSource.GetDefaultView(TrainRoutes);
            RoutesView.Filter = RouteFilter;
            RoutesView.SortDescriptions.Add(
                new SortDescription(
                    nameof(PassengerRoute.DepartureTime),
                    ListSortDirection.Ascending
                )
            );
        }

        private void OnUserSessionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IUserSessionService.ConnectedUser))
            {
                OnPropertyChanged(nameof(ConnectedUser));
            }
        }

        private bool CanReserveExecute()
        {
            if (SelectedRoute == null)
                return false;

            return SelectedRoute.CanReserve;
        }

        private void LoadRoutes()
        {
            try
            {
                ClearErrors("LoadRoutes");
                TrainRoutes.Clear();

                IEnumerable<Itineraire> itineraires;

                if (SelectedDate.HasValue && !string.IsNullOrWhiteSpace(DestinationFilter))
                {
                    itineraires = _itineraireService.GetItinerairesByDate(SelectedDate.Value)
                        .Where(i => i.Etapes != null && i.Etapes.Any(e =>
                            e.Lieu.Contains(DestinationFilter, StringComparison.OrdinalIgnoreCase)));
                }
                else if (SelectedDate.HasValue)
                {
                    itineraires = _itineraireService.GetItinerairesByDate(SelectedDate.Value);
                }
                else if (!string.IsNullOrWhiteSpace(DestinationFilter))
                {
                    itineraires = _itineraireService.GetItinerairesByDestination(DestinationFilter);
                }
                else
                {
                    itineraires = _itineraireService.GetItinerairesDisponibles();
                }

                foreach (Itineraire itineraire in itineraires)
                {
                    if (itineraire != null)
                    {
                        PassengerRoute routeVM = new PassengerRoute(
                            itineraire,
                            _itineraireService);
                        TrainRoutes.Add(routeVM);
                    }
                }

                if (RoutesView != null)
                    RoutesView.Refresh();
            }
            catch (Exception ex)
            {
                AddError("LoadRoutes", $"Erreur : {ex.Message}");
                
            }
        }

        private bool RouteFilter(object item)
        {
            if (item is not PassengerRoute r)
                return false;

            // Date
            if (SelectedDate.HasValue && r.DepartureTime.Date != SelectedDate.Value.Date)
                return false;

            // Heure de départ (simple contient)
            if (
                !string.IsNullOrWhiteSpace(DepartureTimeFilter)
                && !r.DepartureTime.ToString("HH:mm").Contains(DepartureTimeFilter)
            )
                return false;

            // Destination finale
            if (
                !string.IsNullOrWhiteSpace(DestinationFilter)
                && !r.FinalDestination.Contains(
                    DestinationFilter,
                    StringComparison.OrdinalIgnoreCase
                )
            )
                return false;

            return true;
        }

        private void ReserveSelected()
        {
            try
            {

                
                if (SelectedRoute == null)
                {
                    ClearErrors("Reservation");
                    AddError("Reservation", "Veuillez sélectionner un itinéraire.");
                    return;
                }
               
                if (!SelectedRoute.CanReserve)
                {
                    ClearErrors("Reservation");
                    AddError("Reservation", "Cet itinéraire ne peut pas être réservé.");
                    return;
                }
                if (ConnectedUser == null)
                {
                    ClearErrors("Reservation");
                    AddError("Reservation", "Vous devez être connecté pour réserver.");
                    return;
                }
                
                bool success = _itineraireService.CreerReservation(
                     SelectedRoute.ItineraireId,
                     ConnectedUser.Id,
                     1,
                     SelectedRoute.Price
                );

                if (success)
                {
                    System.Windows.MessageBox.Show(
                        $"Réservation confirmée pour le train {SelectedRoute.TrainNumber}!",
                        "Succès",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                    SelectedRoute.RefreshAvailableSeats();
                    LoadRoutes();
                }
                else
                {
                    ClearErrors("Reservation");
                    AddError("Reservation", "Vous avez déja une reservation pour cet itinéraire. ");
                }
            }
            catch (Exception ex)
            {
                ClearErrors("Reservation");
                AddError("Reservation", $"Erreur lors de la réservation: {ex.Message}");
            }
           
        }
    }
}
