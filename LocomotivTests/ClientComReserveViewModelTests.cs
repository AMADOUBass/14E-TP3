using Locomotiv.Model;
using Locomotiv.Utils.Services;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;
using System.Linq;
using Xunit;

namespace LocomotivTests
{
    public class ClientComReserveViewModelTests
    {
        private const decimal INITIAL_PRICE = 1000m;
        private const double INITIAL_CAPACITY = 100;
        private const int INITIAL_WAGONS = 5;

        private CommercialRoute CreerRoute(int wagons = INITIAL_WAGONS, double capacite = INITIAL_CAPACITY, string restrictions = "Aucune.")
        {
            return new CommercialRoute
            {
                TrainNumber = "T-123",
                AvailableWagons = wagons,
                CapacityTons = capacite,
                Price = INITIAL_PRICE,
                MontantReservation = 0m,
                Restrictions = restrictions,
                Status = "Planifié"
            };
        }

        private ClientComReserveViewModel CreerViewModel(CommercialRoute route, int wagonsNecessaires = 1, double poids = 10, double volume = 5, TypeMarchandise type = TypeMarchandise.Bois)
        {
            var mockService = new Mock<IWagonCalculatorService>();

            var mockResult = new CalculReservationResult
            {
                WagonsNecessaires = wagonsNecessaires,
                TarifFinal = 500m,
                Message = "Calcul OK"
            };

            mockService.Setup(s => s.Calculer(
                It.IsAny<CommercialRoute>(),
                It.IsAny<TypeMarchandise>(),
                It.IsAny<double>(),
                It.IsAny<double>()))
                .Returns(mockResult);

            var vm = new ClientComReserveViewModel(route, mockService.Object);

            vm.WeightToReserve = poids;
            vm.VolumeToReserve = volume;
            vm.SelectedTypeMarchandise = type;

            return vm;
        }

        [Fact]
        public void ConfirmeReservationValide_MetAJourRouteEtDeclencheEvenements()
        {
            var route = CreerRoute();
            var vm = CreerViewModel(route, wagonsNecessaires: 1, poids: 10);

            bool confirmationDeclenchee = false;
            vm.RequestConfirmation += (_, __) => confirmationDeclenchee = true;
            bool fermetureDeclenchee = false;
            vm.RequestClose += () => fermetureDeclenchee = true;

            vm.RestrictionAccepted = true;
            vm.ConfirmReservationCommand.Execute(null);

            Assert.True(confirmationDeclenchee, "L'événement RequestConfirmation doit être déclenché.");
            Assert.True(fermetureDeclenchee, "L'événement RequestClose doit être déclenché.");

            Assert.Equal(INITIAL_WAGONS - 1, route.AvailableWagons);
            Assert.Equal(INITIAL_CAPACITY - 10, route.CapacityTons);

            Assert.Equal(INITIAL_PRICE, route.Price);
            Assert.Equal(500m, route.MontantReservation);
            Assert.Equal(500m, route.PriceRestant);
        }


        [Fact]
        public void PoidsZeroOuNegatif_AfficheErreur()
        {
            var route = CreerRoute();
            var vm = CreerViewModel(route, poids: 0);

            vm.RestrictionAccepted = true;
            vm.ConfirmReservationCommand.Execute(null);

            Assert.Equal("Le poids doit être supérieur à 0.", vm.WeightError);
            Assert.Equal(INITIAL_WAGONS, route.AvailableWagons);
        }

        [Fact]
        public void VolumeZeroOuNegatif_AfficheErreur()
        {
            var route = CreerRoute();
            var vm = CreerViewModel(route, volume: 0);

            vm.RestrictionAccepted = true;
            vm.ConfirmReservationCommand.Execute(null);

            Assert.Equal("Le volume doit être supérieur à 0.", vm.VolumeError);
            Assert.Equal(INITIAL_CAPACITY, route.CapacityTons);
        }

        [Fact]
        public void TropDeWagonsDemandes_AfficheErreur()
        {
            var route = CreerRoute(wagons: 2);
            var vm = CreerViewModel(route, wagonsNecessaires: 3);

            vm.RestrictionAccepted = true;
            vm.ConfirmReservationCommand.Execute(null);

            Assert.Contains("wagons disponibles", vm.WagonsError);
            Assert.Equal(2, route.AvailableWagons);
        }

        [Fact]
        public void RestrictionNonAcceptee_AfficheErreur()
        {
            var route = CreerRoute(restrictions: "Fragile");
            var vm = CreerViewModel(route);

            vm.RestrictionAccepted = false;
            vm.ConfirmReservationCommand.Execute(null);

            Assert.Equal("Vous devez accepter la restriction pour poursuivre.", vm.RestrictionError);
            Assert.Equal(INITIAL_WAGONS, route.AvailableWagons);
        }

        [Fact]
        public void PoidsTropEleve_AfficheErreur()
        {
            var route = CreerRoute(capacite: 50);
            var vm = CreerViewModel(route, poids: 60);

            vm.RestrictionAccepted = true;
            vm.ConfirmReservationCommand.Execute(null);

            Assert.Equal($"Impossible de réserver 60 tonnes, il ne reste que 50 tonnes disponibles.", vm.WeightError);
            Assert.Equal(50, route.CapacityTons);
        }


        [Fact]
        public void ClearErrors_ResetToutesErreursAvantValidation()
        {
            var route = CreerRoute();
            var vm = CreerViewModel(route, poids: 0);

            vm.WeightError = "Erreur Poids";
            vm.VolumeError = "Erreur Volume";
            vm.WagonsError = "Erreur Wagons";
            vm.RestrictionError = "Erreur Restriction";

            vm.ConfirmReservationCommand.Execute(null);

            Assert.Equal("Le poids doit être supérieur à 0.", vm.WeightError);
            Assert.Equal("", vm.VolumeError);
            Assert.Equal("", vm.WagonsError);
            Assert.Equal("", vm.RestrictionError);
        }
    }
}