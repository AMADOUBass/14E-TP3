using Locomotiv.Data;
using Locomotiv.Model;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class ClientComDashboardViewModel : BaseViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IDatabaseSeeder _seeder;

        public User? ConnectedUser => _userSessionService?.ConnectedUser;

        public ObservableCollection<CommercialRoute> CommercialRoutes { get; } = new();
        public ICollectionView RoutesView { get; private set; }

        private string _selectedMarchandiseType;
        public string SelectedMarchandisesType
        {
            get => _selectedMarchandiseType;
            set
            {
                if (_selectedMarchandiseType == value) return;
                _selectedMarchandiseType = value;
                OnPropertyChanged(nameof(SelectedMarchandisesType));
                RoutesView.Refresh();
            }
        }

        private CommercialRoute _selectedRoute;
        public CommercialRoute SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                _selectedRoute = value;
                OnPropertyChanged(nameof(SelectedRoute));
                OnPropertyChanged(nameof(CanReserveSelectedRoute));
            }
        }

        public bool CanReserveSelectedRoute => SelectedRoute?.CanReserve == true;

        public ICommand ReserveCommand => new RelayCommand(OpenReserveWindow, () => CanReserveSelectedRoute);

        public ICommand ResetFiltersCommand => new RelayCommand(() =>
        {
            SelectedMarchandisesType = null;
            SelectedDate = null;
            MinCapacityTons = null;
            MinWagons = null;
            MaxPrice = null;
        });

        public ICommand SearchCommand => new RelayCommand(() => RoutesView.Refresh(), () => true);

        public ICommand SortCommand => new RelayCommand(() => SetSort(nameof(CommercialRoute.DepartureTime), ListSortDirection.Ascending), () => true);
        public ICommand SortDescendingCommand => new RelayCommand(() => SetSort(nameof(CommercialRoute.DepartureTime), ListSortDirection.Descending), () => true);

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

        private double? _minCapacityTons;
        public double? MinCapacityTons
        {
            get => _minCapacityTons;
            set
            {
                _minCapacityTons = value;
                OnPropertyChanged(nameof(MinCapacityTons));
                RoutesView.Refresh();
            }
        }

        private int? _minWagons;
        public int? MinWagons
        {
            get => _minWagons;
            set
            {
                _minWagons = value;
                OnPropertyChanged(nameof(MinWagons));
                RoutesView.Refresh();
            }
        }

        private decimal? _maxPrice;
        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                _maxPrice = value;
                OnPropertyChanged(nameof(MaxPrice));
                RoutesView.Refresh();
            }
        }
        public ObservableCollection<string> MarchandisesTypes { get; } = new()
        {
            "Conteneurs",
            "Véhicules",
            "Produits chimiques",
            "Matériaux de construction",
            "Produits agricoles",
            "Produits manufacturés",
        };

        public ClientComDashboardViewModel(IUserSessionService userSessionService, IDatabaseSeeder seeder)
        {
            _userSessionService = userSessionService;
            _seeder = seeder;

            _userSessionService.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IUserSessionService.ConnectedUser))
                    OnPropertyChanged(nameof(ConnectedUser));
            };

            foreach (var route in _seeder.GetMockRoutes())
            {
                CommercialRoutes.Add(route);

                route.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CommercialRoute.PriceRestant)
                        || e.PropertyName == nameof(CommercialRoute.AvailableWagons)
                        || e.PropertyName == nameof(CommercialRoute.CapacityTons))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RoutesView.Refresh();
                            OnPropertyChanged(nameof(CanReserveSelectedRoute));
                            OnPropertyChanged(nameof(SelectedRoute));
                        });
                    }
                };
            }

            RoutesView = CollectionViewSource.GetDefaultView(CommercialRoutes);
            RoutesView.Filter = RouteFilter;
            RoutesView.SortDescriptions.Add(new SortDescription(nameof(CommercialRoute.DepartureTime), ListSortDirection.Ascending));
            RoutesView.SortDescriptions.Add(new SortDescription(nameof(CommercialRoute.Price), ListSortDirection.Ascending));
        }

        private bool RouteFilter(object item)
        {
            if (item is not CommercialRoute r) return false;

            if (!string.IsNullOrWhiteSpace(SelectedMarchandisesType) &&
                !string.Equals(r.MarchandisesType, SelectedMarchandisesType, StringComparison.OrdinalIgnoreCase))
                return false;

            if (SelectedDate.HasValue && r.DepartureTime.Date != SelectedDate.Value.Date)
                return false;

            if (MinCapacityTons.HasValue && r.CapacityTons < MinCapacityTons.Value)
                return false;

            if (MinWagons.HasValue && r.AvailableWagons < MinWagons.Value)
                return false;

            if (MaxPrice.HasValue && r.PriceRestant > MaxPrice.Value)
                return false;

            return true;
        }

        private void OpenReserveWindow()
        {
            if (SelectedRoute == null) return;

            var calculator = new WagonCalculatorService();
            var vm = new ClientComReserveViewModel(SelectedRoute, calculator);

            vm.RequestClose += () =>
            {
                RoutesView.Refresh();
                OnPropertyChanged(nameof(SelectedRoute));
                OnPropertyChanged(nameof(CanReserveSelectedRoute));
            };

            var win = new Locomotiv.View.ClientComReserveView(vm);
            win.ShowDialog();
        }

        public void SetSort(string propertyName, ListSortDirection direction)
        {
            RoutesView.SortDescriptions.Clear();
            RoutesView.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
    }
}