using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class PassengerRoute : BaseViewModel
    {
        private readonly Itineraire _itineraire;
        private readonly IItineraireService _itineraireService;
        private static readonly Random _random = new Random();

        public int ItineraireId => _itineraire.Id;

        public string TrainNumber => _itineraire.Train?.Nom ?? "Non disponible";

        public DateTime DepartureTime => _itineraire.DateDepart;
        public DateTime ArrivalTime => _itineraire.DateArrivee;

        public string FinalDestination =>
            _itineraire.Etapes?.OrderByDescending(e => e.Ordre).FirstOrDefault()?.Lieu ?? "Non disponible";

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

        public decimal Price { get; set; } = _random.Next(10, 80);

        public bool CanReserve => AvailableSeats > 0 && DepartureTime > DateTime.Now;

        public PassengerRoute(Itineraire itineraire, IItineraireService itineraireService)
        {
            _itineraire = itineraire ?? throw new ArgumentNullException(nameof(itineraire));
            _itineraireService = itineraireService ?? throw new ArgumentNullException(nameof(itineraireService));
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
        private readonly IDialogService _dialogService;

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

        public ICommand SearchCommand { get; }
        public ICommand ReserveCommand { get; }

        public ClientDashboardViewModel(
            IUserSessionService userSessionService,
            IItineraireService itineraireService,
            IDialogService dialogService)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            _itineraireService = itineraireService ?? throw new ArgumentNullException(nameof(itineraireService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            SearchCommand = new RelayCommand(LoadRoutes);
            ReserveCommand = new RelayCommand(ReserveSelected, CanReserveExecute);

            _userSessionService.PropertyChanged += OnUserSessionChanged;
            LoadRoutes();

            RoutesView = CollectionViewSource.GetDefaultView(TrainRoutes);
            RoutesView.Filter = RouteFilter;
        }

        private void OnUserSessionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IUserSessionService.ConnectedUser))
                OnPropertyChanged(nameof(ConnectedUser));
        }

        private bool CanReserveExecute() => SelectedRoute?.CanReserve ?? false;

        private void LoadRoutes()
        {
            TrainRoutes.Clear();
            var itineraires = _itineraireService.GetItinerairesDisponibles();

            foreach (var itin in itineraires)
            {
                TrainRoutes.Add(new PassengerRoute(itin, _itineraireService));
            }

            RoutesView?.Refresh();
        }

        private bool RouteFilter(object item) => true; // pour tests simplifiés

        private void ReserveSelected()
        {
            if (SelectedRoute == null)
            {
                AddError("Reservation", "Veuillez sélectionner un itinéraire.");
                return;
            }

            if (!SelectedRoute.CanReserve)
            {
                AddError("Reservation", "Cet itinéraire ne peut pas être réservé.");
                return;
            }

            if (ConnectedUser == null)
            {
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
                _dialogService.ShowMessage(
                    $"Réservation confirmée pour le train {SelectedRoute.TrainNumber} !",
                    "Succès"
                );
                SelectedRoute.RefreshAvailableSeats();
                LoadRoutes();
            }
            else
            {
                AddError("Reservation", "Vous avez déjà une réservation pour cet itinéraire.");
            }
        }
    }
}
