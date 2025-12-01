using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Locomotiv.Model; // pour User
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces; // pour IUserSessionService

namespace Locomotiv.ViewModel
{
    public class PassengerRoute
    {
        public string TrainNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string IntermediateStations { get; set; }
        public string FinalDestination { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } // "Planifié", "En cours", "Terminé"
        public decimal Price { get; set; }

        // Pour bouton Réserver
        public bool CanReserve { get; set; }
    }

    public class ClientDashboardViewModel : BaseViewModel
    {
        private readonly IUserSessionService _userSessionService;

        public User? ConnectedUser => _userSessionService.ConnectedUser;

        // Données mock
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
                RoutesView.Refresh();
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
                RoutesView.Refresh();
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
                RoutesView.Refresh();
            }
        }

        // Commandes
        public ICommand SearchCommand => new RelayCommand(() => RoutesView.Refresh(), canSearch);
        public ICommand ReserveCommand =>
            new RelayCommand(() => ReserveSelected(), () => SelectedRoute?.CanReserve == true);

        public bool canSearch() => true;

        public ClientDashboardViewModel(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;

            _userSessionService.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IUserSessionService.ConnectedUser))
                    OnPropertyChanged(nameof(ConnectedUser));
            };

            SeedMockRoutes();

            RoutesView = CollectionViewSource.GetDefaultView(TrainRoutes);
            RoutesView.Filter = RouteFilter;

            RoutesView.SortDescriptions.Add(
                new SortDescription(
                    nameof(PassengerRoute.DepartureTime),
                    ListSortDirection.Ascending
                )
            );
        }

        private void SeedMockRoutes()
        {
            TrainRoutes.Add(
                new PassengerRoute
                {
                    TrainNumber = "P-101",
                    DepartureTime = DateTime.Today.AddHours(8),
                    ArrivalTime = DateTime.Today.AddHours(12),
                    IntermediateStations = "Trois-Rivières, Drummondville",
                    FinalDestination = "Montréal",
                    AvailableSeats = 45,
                    Status = "Planifié",
                    Price = 75m,
                    CanReserve = true,
                }
            );

            TrainRoutes.Add(
                new PassengerRoute
                {
                    TrainNumber = "P-202",
                    DepartureTime = DateTime.Today.AddHours(10),
                    ArrivalTime = DateTime.Today.AddHours(14),
                    IntermediateStations = "Laval, Longueuil",
                    FinalDestination = "Québec",
                    AvailableSeats = 20,
                    Status = "En cours",
                    Price = 90m,
                    CanReserve = true,
                }
            );

            TrainRoutes.Add(
                new PassengerRoute
                {
                    TrainNumber = "P-303",
                    DepartureTime = DateTime.Today.AddHours(6),
                    ArrivalTime = DateTime.Today.AddHours(9),
                    IntermediateStations = "Ottawa",
                    FinalDestination = "Toronto",
                    AvailableSeats = 0,
                    Status = "Terminé",
                    Price = 120m,
                    CanReserve = false,
                }
            );
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
            if (SelectedRoute != null && SelectedRoute.CanReserve)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Réservation effectuée pour {SelectedRoute.TrainNumber}"
                );
            }
        }
    }
}
