using Locomotiv.Model;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Locomotiv.Model.enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocomotivTests
{
    public class ClientDashboardViewModelTests
    {
        private readonly Mock<IUserSessionService> _userSessionMock;
        private readonly Mock<IItineraireService> _itineraireServiceMock;
        private readonly User _testUser;

        public ClientDashboardViewModelTests() 
        { 
            _userSessionMock = new Mock<IUserSessionService>();
            _itineraireServiceMock = new Mock<IItineraireService>();

            _testUser = new User
            {
                Id = 1,
                Prenom = "Amadou",
                Nom = "Bassoum",
                Role = UserRole.Client

            };
        
        }

        [Fact]
        public void Constructor_AvecParametresValides_InitialiseCorrectement()
        {
            // Arrange & Act
            var viewModel = CreerViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.TrainRoutes);
            Assert.NotNull(viewModel.SearchCommand);
            Assert.NotNull(viewModel.ReserveCommand);
        }

        [Fact]
        public void Constructor_SansUserSession_LanceException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ClientDashboardViewModel(null, _itineraireServiceMock.Object));
        }

        [Fact]
        public void Constructor_SansItineraireService_LanceException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ClientDashboardViewModel(_userSessionMock.Object, null));
        }

        [Fact]
        public void ReserveSelected_SansItineraireSelectionne_AjouteErreur()
        {
            // Arrange
            _userSessionMock.Setup(s => s.ConnectedUser)
                .Returns(_testUser);

            var viewModel = CreerViewModel();

            // Act
            viewModel.ReserveCommand.Execute(null);

            // Assert
            Assert.True(viewModel.HasErrors);
            Assert.Contains("sélectionner", viewModel.ErrorMessages);
        }

        [Fact]
        public void ReserveSelected_AvecSucces_AppelleService()
        {
            // Arrange
            var itineraire = CreerItineraireTest(1, DateTime.Now.AddHours(2));
            _itineraireServiceMock.Setup(s => s.GetItinerairesDisponibles())
                .Returns(new List<Itineraire> { itineraire });
            _itineraireServiceMock.Setup(s => s.GetPlacesDisponibles(1))
                .Returns(10);
            _itineraireServiceMock.Setup(s => s.CreerReservation(1, 1, 1, It.IsAny<decimal>()))
                .Returns(true);

            var viewModel = CreerViewModel();
            viewModel.SearchCommand.Execute(null);
            viewModel.SelectedRoute = viewModel.TrainRoutes.First();

            // Act
            viewModel.ReserveCommand.Execute(null);

            // Assert
            _itineraireServiceMock.Verify(
                s => s.CreerReservation(1, 1, 1, It.IsAny<decimal>()),
                Times.Once);
        }


        [Fact]
        public void LoadRoutes_AvecItinerairesDisponibles_ChargeListeCorrectement()
        {
            // Arrange
            var itineraires = CreerItinerairesTest(3);
            _itineraireServiceMock.Setup(s => s.GetItinerairesDisponibles())
                .Returns(itineraires);
            _itineraireServiceMock.Setup(s => s.GetPlacesDisponibles(It.IsAny<int>()))
                .Returns(50);

            var viewModel = CreerViewModel();

            // Act
            viewModel.SearchCommand.Execute(null);

            // Assert
            Assert.Equal(3, viewModel.TrainRoutes.Count);
        }

        [Fact]
        public void LoadRoutes_SansAucunItineraire_ListesVide()
        {
            // Arrange
            _itineraireServiceMock
                .Setup(s => s.GetItinerairesDisponibles())
                .Returns(new List<Itineraire>());

            var viewModel = CreerViewModel();

            // Act
            viewModel.SearchCommand.Execute(null);

            // Assert
            Assert.Empty(viewModel.TrainRoutes);
        }





        private ClientDashboardViewModel CreerViewModel()
        {
            _userSessionMock.Setup(s => s.ConnectedUser).Returns(_testUser);
            return new ClientDashboardViewModel(
                _userSessionMock.Object,
                _itineraireServiceMock.Object

            );
        }


        private Itineraire CreerItineraireTest(int id, DateTime dateDepart)
        {
            return new Itineraire
            {
                Id = id,
                Nom = $"Itinéraire {id}",
                DateDepart = dateDepart,
                DateArrivee = dateDepart.AddHours(4),
                TrainId = id,
                Train = new Train
                {
                    Id = id,
                    Nom = $"Train {id}",
                    Capacite = 100,
                    Etat = EtatTrain.EnGare
                },
                Etapes = new List<Etape>
                {
                    new Etape { Id = id * 10 + 1, Lieu = "Montréal", Ordre = 1, ItineraireId = id, TrainId = id },
                    new Etape { Id = id * 10 + 2, Lieu = "Québec", Ordre = 2, ItineraireId = id, TrainId = id }
                }
            };
        }

        private List<Itineraire> CreerItinerairesTest(int nombre)
        {
            var itineraires = new List<Itineraire>();
            for (int i = 1; i <= nombre; i++)
            {
                itineraires.Add(CreerItineraireTest(i, DateTime.Now.AddDays(i)));
            }
            return itineraires;
        }
    }
}
