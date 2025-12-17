using System;
using System.ComponentModel;

namespace Locomotiv.Model
{
    public class CommercialRoute : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _availableWagons;
        public int AvailableWagons
        {
            get => _availableWagons;
            set
            {
                if (_availableWagons != value)
                {
                    _availableWagons = value;
                    OnPropertyChanged(nameof(AvailableWagons));
                    OnPropertyChanged(nameof(CanReserve));
                }
            }
        }

        private double _capacityTons;
        public double CapacityTons
        {
            get => _capacityTons;
            set
            {
                if (_capacityTons != value)
                {
                    _capacityTons = value;
                    OnPropertyChanged(nameof(CapacityTons));
                    OnPropertyChanged(nameof(CanReserve));
                }
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                    OnPropertyChanged(nameof(PriceRestant));
                }
            }
        }

        private decimal _montantReservation = 0m;
        public decimal MontantReservation
        {
            get => _montantReservation;
            set
            {
                if (_montantReservation != value)
                {
                    _montantReservation = value;
                    OnPropertyChanged(nameof(MontantReservation));
                    OnPropertyChanged(nameof(PriceRestant));
                }
            }
        }
        public decimal PriceRestant => Price - MontantReservation;

        public string TrainNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string TransitStations { get; set; }
        public string Status { get; set; }
        public string EstimatedDelivery { get; set; }
        public string Restrictions { get; set; }
        public string MarchandisesType { get; set; }

        public bool CanReserve => Status != "Terminé" && AvailableWagons > 0 && CapacityTons > 0 && PriceRestant > 0;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}