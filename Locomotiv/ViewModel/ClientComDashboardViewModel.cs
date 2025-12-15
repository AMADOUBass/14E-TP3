using Locomotiv.Model;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class CommercialRoute
    {
        public string TrainNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string TransitStations { get; set; }
        public int AvailableWagons { get; set; }
        public double CapacityTons { get; set; }
        public string Status { get; set; }
        public string EstimatedDelivery { get; set; }
        public decimal Price { get; set; }
        public string Restrictions { get; set; }

        public string GoodsType { get; set; }

        public bool CanReserve => Status != "Terminé" && AvailableWagons > 0 && CapacityTons > 0;
    }

    public class ClientComDashboardViewModel : BaseViewModel
    {
        private readonly IUserSessionService _userSessionService;

        public User? ConnectedUser => _userSessionService.ConnectedUser;

        public ObservableCollection<string> GoodsTypes { get; } = new()
        {
            "Conteneurs",
            "Véhicules",
            "Produits chimiques",
            "Matériaux de construction",
            "Produits agricoles",
            "Produits manufacturés",
        };

        private string _selectedGoodsType;
        public string SelectedGoodsType
        {
            get => _selectedGoodsType;
            set
            {
                if (_selectedGoodsType == value) return;
                _selectedGoodsType = value;
                OnPropertyChanged(nameof(SelectedGoodsType));
                RoutesView?.Refresh();
            }
        }

        public ObservableCollection<CommercialRoute> CommercialRoutes { get; } = new();
        public ICollectionView RoutesView { get; }

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

        private void OpenReserveWindow()
        {
            if (SelectedRoute == null) return;

            var win = new ClientComReserveView();
            win.DataContext = new ClientComReserveViewModel(SelectedRoute);
            win.ShowDialog();

            RoutesView.Refresh();
            OnPropertyChanged(nameof(SelectedRoute));
            OnPropertyChanged(nameof(CanReserveSelectedRoute));
        }

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

        public ICommand ResetFiltersCommand => new RelayCommand(() =>
        {
            SelectedGoodsType = null;
            SelectedDate = null;
            MinCapacityTons = null;
            MinWagons = null;
            MaxPrice = null;
        });

        public ICommand SearchCommand => new RelayCommand(() => RoutesView.Refresh(), () => true);

        public ICommand SortCommand => new RelayCommand(() => SetSort(nameof(CommercialRoute.DepartureTime), ListSortDirection.Ascending), () => true);
        public ICommand SortDescendingCommand => new RelayCommand(() => SetSort(nameof(CommercialRoute.DepartureTime), ListSortDirection.Descending), () => true);

        public ClientComDashboardViewModel(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;

            _userSessionService.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IUserSessionService.ConnectedUser))
                    OnPropertyChanged(nameof(ConnectedUser));
            };

            SeedMockRoutes();

            RoutesView = CollectionViewSource.GetDefaultView(CommercialRoutes);
            RoutesView.Filter = RouteFilter;
            RoutesView.SortDescriptions.Add(new SortDescription(nameof(CommercialRoute.DepartureTime), ListSortDirection.Ascending));
            RoutesView.SortDescriptions.Add(new SortDescription(nameof(CommercialRoute.Price), ListSortDirection.Ascending));
        }

        private void SeedMockRoutes()
        {
            CommercialRoutes.Add(new CommercialRoute
            {
                TrainNumber = "T-101",
                DepartureTime = DateTime.Now.AddHours(1),
                ArrivalTime = DateTime.Now.AddHours(5),
                TransitStations = "Montréal, Trois-Rivières",
                AvailableWagons = 12,
                CapacityTons = 250,
                Status = "En cours",
                EstimatedDelivery = "2 jours",
                Price = 1500m,
                Restrictions = "Pas de matières dangereuses.",
                GoodsType = "Conteneurs",
            });

            CommercialRoutes.Add(new CommercialRoute
            {
                TrainNumber = "T-202",
                DepartureTime = DateTime.Now.AddHours(2),
                ArrivalTime = DateTime.Now.AddHours(7),
                TransitStations = "Québec, Lévis",
                AvailableWagons = 8,
                CapacityTons = 180,
                Status = "Planifié",
                EstimatedDelivery = "3 jours",
                Price = 1200m,
                Restrictions = "Température contrôlée requise.",
                GoodsType = "Véhicules",
            });

            CommercialRoutes.Add(new CommercialRoute
            {
                TrainNumber = "T-303",
                DepartureTime = DateTime.Now.AddHours(-1),
                ArrivalTime = DateTime.Now.AddHours(3),
                TransitStations = "Ottawa",
                AvailableWagons = 0,
                CapacityTons = 0,
                Status = "Terminé",
                EstimatedDelivery = "1 jour",
                Price = 2000m,
                Restrictions = "Aucune.",
                GoodsType = "Produits chimiques",
            });

            CommercialRoutes.Add(new CommercialRoute
            {
                TrainNumber = "T-404",
                DepartureTime = DateTime.Now.AddHours(6),
                ArrivalTime = DateTime.Now.AddHours(12),
                TransitStations = "Saguenay, Rimouski",
                AvailableWagons = 5,
                CapacityTons = 120,
                Status = "Planifié",
                EstimatedDelivery = "4 jours",
                Price = 900m,
                Restrictions = "Fragile.",
                GoodsType = "Produits agricoles",
            });
        }

        private bool RouteFilter(object item)
        {
            if (item is not CommercialRoute r) return false;

            if (!string.IsNullOrWhiteSpace(SelectedGoodsType) &&
                !string.Equals(r.GoodsType, SelectedGoodsType, StringComparison.OrdinalIgnoreCase))
                return false;

            if (SelectedDate.HasValue && r.DepartureTime.Date != SelectedDate.Value.Date)
                return false;

            if (MinCapacityTons.HasValue && r.CapacityTons < MinCapacityTons.Value)
                return false;

            if (MinWagons.HasValue && r.AvailableWagons < MinWagons.Value)
                return false;

            if (MaxPrice.HasValue && r.Price > MaxPrice.Value)
                return false;

            return true;
        }

        public void SetSort(string propertyName, ListSortDirection direction)
        {
            RoutesView.SortDescriptions.Clear();
            RoutesView.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
    }
}