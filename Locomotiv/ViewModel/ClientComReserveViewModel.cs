using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Locomotiv.ViewModel
{
    public class ClientComReserveViewModel : BaseViewModel
    {
        public CommercialRoute Route { get; }

        private int? _wagonsToReserve;
        private double? _weightToReserve;
        private bool _restrictionAccepted;

        private string _wagonsError;
        private string _weightError;
        private string _restrictionError;

        public int? WagonsToReserve
        {
            get => _wagonsToReserve;
            set
            {
                SetProperty(ref _wagonsToReserve, value);
                WagonsError = string.Empty;
            }
        }

        public double? WeightToReserve
        {
            get => _weightToReserve;
            set
            {
                SetProperty(ref _weightToReserve, value);
                WeightError = string.Empty;
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

        public string WagonsError
        {
            get => _wagonsError;
            set => SetProperty(ref _wagonsError, value);
        }

        public string WeightError
        {
            get => _weightError;
            set => SetProperty(ref _weightError, value);
        }

        public string RestrictionError
        {
            get => _restrictionError;
            set => SetProperty(ref _restrictionError, value);
        }

        public string RestrictionText =>
            string.IsNullOrWhiteSpace(Route.Restrictions) || Route.Restrictions == "Aucune."
                ? string.Empty
                : Route.Restrictions;

        public bool IsRestrictionVisible => !string.IsNullOrEmpty(RestrictionText);

        public ICommand ConfirmReservationCommand { get; }

        public ClientComReserveViewModel(CommercialRoute route)
        {
            Route = route;
            ConfirmReservationCommand = new RelayCommand(Confirm, () => true);
        }

        private void Confirm()
        {
            WagonsError = "";
            WeightError = "";
            RestrictionError = "";

            if (!Validate())
                return;

            Route.AvailableWagons -= WagonsToReserve.Value;
            Route.CapacityTons -= WeightToReserve.Value;

            MessageBox.Show(
                $"Réservation confirmée !\n\n" +
                $"Train : {Route.TrainNumber}\n" +
                $"Wagons : {WagonsToReserve}\n" +
                $"Poids : {WeightToReserve} tonnes",
                "Succès",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            CloseWindow();
        }

        private bool Validate()
        {
            bool valid = true;

            if (!WagonsToReserve.HasValue || WagonsToReserve.Value <= 0)
            {
                WagonsError = "Le nombre de wagons doit être supérieur à 0.";
                valid = false;
            }
            else if (WagonsToReserve.Value > Route.AvailableWagons)
            {
                WagonsError = $"Il ne reste que {Route.AvailableWagons} wagons disponibles.";
                valid = false;
            }

            if (!WeightToReserve.HasValue || WeightToReserve.Value <= 0)
            {
                WeightError = "Le poids doit être supérieur à 0.";
                valid = false;
            }
            else if (WeightToReserve.Value > Route.CapacityTons)
            {
                WeightError = $"Poids maximal autorisé : {Route.CapacityTons} tonnes.";
                valid = false;
            }

            if (IsRestrictionVisible && !RestrictionAccepted)
            {
                RestrictionError = "Vous devez accepter la restriction pour poursuivre.";
                valid = false;
            }

            return valid;
        }

        private void CloseWindow()
        {
            Application.Current.Windows
                .OfType<Window>()
                .SingleOrDefault(w => w.DataContext == this)?
                .Close();
        }
    }
}