using Locomotiv.Model;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class ClientComReserveViewModel : BaseViewModel
    {
        private readonly IWagonCalculatorService _calculatorService;

        public CommercialRoute Route { get; }

        private double? _weightToReserve;
        private double? _volumeToReserve;
        private TypeMarchandise _selectedTypeMarchandise;
        private bool _restrictionAccepted;

        private string _weightError;
        private string _volumeError;
        private string _wagonsError;
        private string _restrictionError;

        public int WagonsNecessaires { get; private set; }
        public decimal TarifReservation { get; private set; }
        public string Message { get; private set; }

        public ICommand ConfirmReservationCommand { get; }

        public event Action RequestClose;
        public event Action<string, string> RequestConfirmation;

        public ClientComReserveViewModel(CommercialRoute route, IWagonCalculatorService calculatorService)
        {
            Route = route;
            _calculatorService = calculatorService;
            ConfirmReservationCommand = new RelayCommand(Confirm);
        }

        public double? WeightToReserve
        {
            get => _weightToReserve;
            set
            {
                SetProperty(ref _weightToReserve, value);
                WeightError = string.Empty;
                UpdateWagonsNecessaires();
            }
        }

        public double? VolumeToReserve
        {
            get => _volumeToReserve;
            set
            {
                SetProperty(ref _volumeToReserve, value);
                VolumeError = string.Empty;
                UpdateWagonsNecessaires();
            }
        }

        public TypeMarchandise SelectedTypeMarchandise
        {
            get => _selectedTypeMarchandise;
            set
            {
                SetProperty(ref _selectedTypeMarchandise, value);
                UpdateWagonsNecessaires();
            }
        }

        public bool RestrictionAccepted
        {
            get => _restrictionAccepted;
            set
            {
                SetProperty(ref _restrictionAccepted, value);
                RestrictionError = string.Empty;
            }
        }

        public string WeightError { get => _weightError; set => SetProperty(ref _weightError, value); }
        public string VolumeError { get => _volumeError; set => SetProperty(ref _volumeError, value); }
        public string WagonsError { get => _wagonsError; set => SetProperty(ref _wagonsError, value); }
        public string RestrictionError { get => _restrictionError; set => SetProperty(ref _restrictionError, value); }

        public string RestrictionText =>
            string.IsNullOrWhiteSpace(Route.Restrictions) || Route.Restrictions == "Aucune."
                ? string.Empty
                : Route.Restrictions;

        public bool IsRestrictionVisible => !string.IsNullOrEmpty(RestrictionText);

        private void UpdateWagonsNecessaires()
        {
            WagonsError = string.Empty;

            if (!WeightToReserve.HasValue || !VolumeToReserve.HasValue || SelectedTypeMarchandise == null)
                return;

            var result = _calculatorService.Calculer(Route, SelectedTypeMarchandise, WeightToReserve.Value, VolumeToReserve.Value);

            WagonsNecessaires = result.WagonsNecessaires;
            TarifReservation = result.TarifFinal;
            Message = result.Message;

            if (WagonsNecessaires > Route.AvailableWagons)
                WagonsError = $"Il ne reste que {Route.AvailableWagons} wagons disponibles. Réduisez le poids ou le volume.";

            OnPropertyChanged(nameof(WagonsNecessaires));
            OnPropertyChanged(nameof(TarifReservation));
            OnPropertyChanged(nameof(Message));
        }

        private void Confirm()
        {
            ClearErrors();

            if (!Validate())
                return;

            double poidsARetirer = Math.Min(WeightToReserve.Value, Route.CapacityTons);
            int wagonsARetirer = Math.Min(WagonsNecessaires, Route.AvailableWagons);

            Route.AvailableWagons -= wagonsARetirer;
            Route.CapacityTons -= poidsARetirer;
            Route.MontantReservation += TarifReservation;

            var confirmationMessage =
                $"Train : {Route.TrainNumber}\n" +
                $"Wagons réservés : {wagonsARetirer}\n" +
                $"Poids : {poidsARetirer} tonnes\n" +
                $"Volume : {VolumeToReserve} m³\n" +
                $"Prix : {TarifReservation:C}";

            RequestConfirmation?.Invoke(confirmationMessage, "Réservation confirmée !");
            RequestClose?.Invoke();

            TarifReservation = Route.PriceRestant;

            OnPropertyChanged(nameof(TarifReservation));
        }

        private void ClearErrors()
        {
            WeightError = "";
            VolumeError = "";
            WagonsError = "";
            RestrictionError = "";
        }

        private bool Validate()
        {
            bool valid = true;

            if (!WeightToReserve.HasValue || WeightToReserve.Value <= 0)
            {
                WeightError = "Le poids doit être supérieur à 0.";
                valid = false;
            }
            else if (WeightToReserve.Value > Route.CapacityTons)
            {
                WeightError = $"Impossible de réserver {WeightToReserve.Value} tonnes, il ne reste que {Route.CapacityTons} tonnes disponibles.";
                valid = false;
            }

            if (!VolumeToReserve.HasValue || VolumeToReserve.Value <= 0)
            {
                VolumeError = "Le volume doit être supérieur à 0.";
                valid = false;
            }

            if (WagonsNecessaires > Route.AvailableWagons)
            {
                WagonsError = $"Il ne reste que {Route.AvailableWagons} wagons disponibles.";
                valid = false;
            }

            if (IsRestrictionVisible && !RestrictionAccepted)
            {
                RestrictionError = "Vous devez accepter la restriction pour poursuivre.";
                valid = false;
            }

            return valid;
        }
    }
}