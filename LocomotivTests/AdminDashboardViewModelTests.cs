using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace Locomotiv.Tests.ViewModel
{
    public class AdminDashboardViewModelTests
    {
        private readonly Mock<ITrainDAL> _trainDalMock;
        private readonly Mock<IStationDAL> _stationDalMock;
        private readonly Mock<IDialogService> _dialogMock;
        private readonly Mock<IBlockDAL> _blockDalMock;
        private readonly Mock<IPointArretDAL> _pointArretDalMock;
        private readonly Mock<IItineraireDAL> _itineraireDalMock;
        private readonly Mock<ILogger> _loggerMock;

        public AdminDashboardViewModelTests()
        {
            _trainDalMock = new Mock<ITrainDAL>(MockBehavior.Strict);
            _stationDalMock = new Mock<IStationDAL>(MockBehavior.Strict);
            _dialogMock = new Mock<IDialogService>(MockBehavior.Strict);
            _blockDalMock = new Mock<IBlockDAL>(MockBehavior.Strict);
            _pointArretDalMock = new Mock<IPointArretDAL>(MockBehavior.Strict);
            _itineraireDalMock = new Mock<IItineraireDAL>(MockBehavior.Strict);

            // Logger en Loose pour accepter tous les appels
            _loggerMock = new Mock<ILogger>(MockBehavior.Loose);
            _loggerMock.Setup(l => l.Info(It.IsAny<string>()));
            _loggerMock.Setup(l => l.Warning(It.IsAny<string>()));
            _loggerMock.Setup(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>()));
        }

        /// <summary>
        /// Helper pour créer le VM en configurant les retours des DAL.
        /// </summary>
        private AdminDashboardViewModel CreateViewModel(
            IEnumerable<Train>? trains = null,
            IEnumerable<Station>? stations = null,
            IEnumerable<Block>? blocks = null,
            IEnumerable<PointArret>? points = null
        )
        {
            _trainDalMock
                .Setup(d => d.GetAllTrains())
                .Returns(trains?.ToList() ?? new List<Train>());

            _stationDalMock
                .Setup(d => d.GetAllStations())
                .Returns(stations?.ToList() ?? new List<Station>());

            _blockDalMock
                .Setup(d => d.GetAllBlocks())
                .Returns(blocks?.ToList() ?? new List<Block>());

            _pointArretDalMock
                .Setup(d => d.GetAllPointArrets())
                .Returns(points?.ToList() ?? new List<PointArret>());

            return new AdminDashboardViewModel(
                _trainDalMock.Object,
                _dialogMock.Object,
                _stationDalMock.Object,
                _blockDalMock.Object,
                _pointArretDalMock.Object,
                _itineraireDalMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public void Ctor_Loads_Data_From_DALs()
        {
            var station = new Station { Id = 1, Nom = "S1", CapaciteMaxTrains = 5 };
            var train = new Train { Id = 1, Nom = "T1", Etat = EtatTrain.EnGare, StationId = 1, Station = station };
            var block = new Block { Id = 1, Nom = "B1" };
            var point = new PointArret { Id = 1, Nom = "PA1", Latitude = 1, Longitude = 2 };

            var vm = CreateViewModel(
                trains: new[] { train },
                stations: new[] { station },
                blocks: new[] { block },
                points: new[] { point }
            );

            Assert.Single(vm.Trains);
            Assert.Single(vm.Stations);
            Assert.Single(vm.Blocks);
            Assert.Single(vm.PointsInteret);
        }

        [Fact]
        public void AddTrain_WithAvailableStation_AddsTrain_AndCallsDAL()
        {
            var station = new Station { Id = 1, Nom = "S1", CapaciteMaxTrains = 2 };
            var existingTrain = new Train { Id = 1, Nom = "T1", Etat = EtatTrain.EnGare, StationId = 1, Station = station };
            var vm = CreateViewModel(trains: new[] { existingTrain }, stations: new[] { station });

            var newTrain = new Train { Id = 2, Nom = "T2", Etat = EtatTrain.EnGare, StationId = station.Id, Station = station };
            Train outTrain = newTrain;

            _dialogMock
                .Setup(d => d.ShowTrainDialog(It.IsAny<List<Station>>(), out outTrain))
                .Returns(true);

            _trainDalMock.Setup(d => d.AddTrain(newTrain));

            _dialogMock.Setup(d => d.ShowMessage(It.Is<string>(m => m.Contains("ajouté avec succès")), It.IsAny<string>()));

            vm.AjouterTrainCommand.Execute(null);

            _trainDalMock.Verify(d => d.AddTrain(newTrain), Times.Once);
            Assert.Equal(2, vm.Trains.Count);
            Assert.Contains(vm.Trains, t => t.Nom == "T2");
        }

        [Fact]
        public void PlanifierItineraire_WhenNoTrainAvailable_ShowsMessage_AndDoesNotCallDialog()
        {
            var t1 = new Train { Id = 1, Nom = "T1", Etat = EtatTrain.EnTransit };
            var t2 = new Train { Id = 2, Nom = "T2", Etat = EtatTrain.HorsService };
            var vm = CreateViewModel(trains: new[] { t1, t2 });

            _dialogMock
                .Setup(d => d.ShowMessage(It.Is<string>(m => m.Contains("Aucun train disponible")), It.IsAny<string>()))
                .Verifiable();

            vm.PlanifierItineraireCommand.Execute(null);

            _dialogMock.Verify();

            Train outTrain = null!;
            List<PointArret> outArrets = null!;
            DateTime outDep = default, outArr = default;

            _dialogMock.Verify(
                d => d.ShowPlanifierItineraireDialog(It.IsAny<List<Train>>(), It.IsAny<List<PointArret>>(), out outTrain, out outArrets, out outDep, out outArr),
                Times.Never
            );
        }

        [Fact]
        public void PlanifierItineraire_WhenDialogCancelled_DoesNothing()
        {
            var t1 = new Train { Id = 1, Nom = "T1", Etat = EtatTrain.EnGare };
            var p1 = new PointArret { Id = 1, Nom = "A", Latitude = 1, Longitude = 1 };
            var p2 = new PointArret { Id = 2, Nom = "B", Latitude = 2, Longitude = 2 };

            var vm = CreateViewModel(trains: new[] { t1 }, points: new[] { p1, p2 });

            Train outTrain = null!;
            List<PointArret> outArrets = null!;
            DateTime outDep = default, outArr = default;

            _dialogMock
                .Setup(d => d.ShowPlanifierItineraireDialog(It.IsAny<List<Train>>(), It.IsAny<List<PointArret>>(), out outTrain, out outArrets, out outDep, out outArr))
                .Returns(false);

            _itineraireDalMock
                .Setup(d => d.PlanifierItineraire(It.IsAny<Itineraire>()))
                .Verifiable();

            vm.PlanifierItineraireCommand.Execute(null);

            _itineraireDalMock.Verify(d => d.PlanifierItineraire(It.IsAny<Itineraire>()), Times.Never);
        }
        // ==============================================================
        // 5. GETCONFLITS
        // ==============================================================

        [Fact]
        public void GetConflits_WhenTwoTrainsOnSameBlock_ReturnsOneConflict()
        {
            // Arrange
            var block = new Block
            {
                Id = 1,
                Nom = "B1",
                LatitudeDepart = 46.0,
                LongitudeDepart = -71.0,
                LatitudeArrivee = 46.01,
                LongitudeArrivee = -71.01,
                EstOccupe = true,
            };

            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnTransit,
                BlockId = 1,
                Block = block,
            };
            var t2 = new Train
            {
                Id = 2,
                Nom = "T2",
                Etat = EtatTrain.EnTransit,
                BlockId = 1,
                Block = block,
            };

            var vm = CreateViewModel(trains: new[] { t1, t2 }, blocks: new[] { block });

            // Act
            var conflits = vm.GetConflits();

            // Assert
            Assert.Single(conflits);
            Assert.Equal(1, conflits[0].BlockConflit.Id);
            Assert.Contains(conflits[0].TrainA.Id, new[] { 1, 2 });
            Assert.Contains(conflits[0].TrainB.Id, new[] { 1, 2 });
        }

        [Fact]
        public void GetConflits_WhenBlocksTooClose_ReturnsConflict()
        {
            // Arrange : deux blocks très proches (< 1 km)
            var blockA = new Block
            {
                Id = 1,
                Nom = "B1",
                LatitudeDepart = 46.0,
                LongitudeDepart = -71.0,
                LatitudeArrivee = 46.005,
                LongitudeArrivee = -71.005,
                EstOccupe = true,
            };

            var blockB = new Block
            {
                Id = 2,
                Nom = "B2",
                LatitudeDepart = 46.006,
                LongitudeDepart = -71.006,
                LatitudeArrivee = 46.01,
                LongitudeArrivee = -71.01,
                EstOccupe = true,
            };

            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnTransit,
                BlockId = 1,
                Block = blockA,
            };
            var t2 = new Train
            {
                Id = 2,
                Nom = "T2",
                Etat = EtatTrain.EnTransit,
                BlockId = 2,
                Block = blockB,
            };

            var vm = CreateViewModel(trains: new[] { t1, t2 }, blocks: new[] { blockA, blockB });

            // Act
            var conflits = vm.GetConflits();

            // Assert
            Assert.Single(conflits);
            Assert.Contains(conflits[0].BlockConflit.Id, new[] { 1, 2 });
        }

        // ==============================================================
        // 6. Petites vérifications d’états dérivés
        // ==============================================================

        [Fact]
        public void TrainsEnMouvement_ReturnsOnlyTrainsWithEtatEnTransit()
        {
            // Arrange
            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnTransit,
            };
            var t2 = new Train
            {
                Id = 2,
                Nom = "T2",
                Etat = EtatTrain.EnGare,
            };

            var vm = CreateViewModel(trains: new[] { t1, t2 });

            // Act
            var enMouvement = vm.TrainsEnMouvement;

            // Assert
            Assert.Single(enMouvement);
            Assert.Equal(EtatTrain.EnTransit, enMouvement[0].Etat);
        }

        [Fact]
        public void TrainsDeLaStationSelectionnee_ReturnsOnlyTrainsOfThatStation()
        {
            // Arrange
            var s1 = new Station
            {
                Id = 1,
                Nom = "S1",
                CapaciteMaxTrains = 5,
            };
            var s2 = new Station
            {
                Id = 2,
                Nom = "S2",
                CapaciteMaxTrains = 5,
            };

            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
                StationId = 1,
                Station = s1,
            };
            var t2 = new Train
            {
                Id = 2,
                Nom = "T2",
                Etat = EtatTrain.EnGare,
                StationId = 2,
                Station = s2,
            };

            var vm = CreateViewModel(trains: new[] { t1, t2 }, stations: new[] { s1, s2 });

            // Act
            vm.StationSelectionnee = s1;
            var trainsStation1 = vm.TrainsDeLaStationSelectionnee;

            // Assert
            Assert.Single(trainsStation1);
            Assert.Equal(1, trainsStation1[0].StationId);
        }
    }
}
