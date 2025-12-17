using System;
using System.Collections.Generic;
using System.Linq;
using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;
using Xunit;

namespace LocomotivTests
{
    public class ClientDashboardViewModelTests
    {
        private readonly Mock<IUserSessionService> _userSessionMock = new();
        private readonly Mock<IItineraireService> _itineraireMock = new();
        private readonly Mock<IDialogService> _dialogMock = new();

        private readonly User _user = new() { Id = 1, Prenom = "Amadou", Nom = "Bassoum" };

        private ClientDashboardViewModel CreateVM()
        {
            _userSessionMock.Setup(s => s.ConnectedUser).Returns(_user);
            return new ClientDashboardViewModel(_userSessionMock.Object, _itineraireMock.Object, _dialogMock.Object);
        }

        private Itineraire CreateItineraire(int id)
        {
            return new Itineraire
            {
                Id = id,
                Train = new Train { Id = id, Nom = $"T{id}", Etat = EtatTrain.EnGare },
                DateDepart = DateTime.Now.AddHours(2),
                DateArrivee = DateTime.Now.AddHours(5),
                Etapes = new List<Etape> { new Etape { Id = 1, Lieu = "Montréal", Ordre = 1 } }
            };
        }

        [Fact]
        public void Constructor_LoadsRoutes()
        {
            var itin = CreateItineraire(1);
            _itineraireMock.Setup(i => i.GetItinerairesDisponibles()).Returns(new List<Itineraire> { itin });
            var vm = CreateVM();
            Assert.Single(vm.TrainRoutes);
            Assert.Equal("T1", vm.TrainRoutes.First().TrainNumber);
        }

        [Fact]
        public void ReserveSelected_ShowsDialog_WhenReservationSuccess()
        {
            var itin = CreateItineraire(1);
            _itineraireMock.Setup(i => i.GetItinerairesDisponibles()).Returns(new List<Itineraire> { itin });
            _itineraireMock.Setup(i => i.GetPlacesDisponibles(It.IsAny<int>())).Returns(10);
            _itineraireMock.Setup(i => i.CreerReservation(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>())).Returns(true);

            var vm = CreateVM();
            vm.SelectedRoute = vm.TrainRoutes.First();

            vm.ReserveCommand.Execute(null);

            _dialogMock.Verify(d => d.ShowMessage(It.Is<string>(s => s.Contains("confirmée")), "Succès"), Times.Once);
        }

        [Fact]
        public void ReserveSelected_AddsError_WhenNoRouteSelected()
        {
            var vm = CreateVM();
            vm.ReserveCommand.Execute(null);
            Assert.Contains(vm.GetErrors("Reservation").Cast<string>(), e => e.Contains("sélectionner un itinéraire"));

        }
    }
}