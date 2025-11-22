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

        public AdminDashboardViewModelTests()
        {
            _trainDalMock = new Mock<ITrainDAL>(MockBehavior.Strict);
            _stationDalMock = new Mock<IStationDAL>(MockBehavior.Strict);
            _dialogMock = new Mock<IDialogService>(MockBehavior.Strict);
            _blockDalMock = new Mock<IBlockDAL>(MockBehavior.Strict);
            _pointArretDalMock = new Mock<IPointArretDAL>(MockBehavior.Strict);
            _itineraireDalMock = new Mock<IItineraireDAL>(MockBehavior.Strict);
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
                _itineraireDalMock.Object
            );
        }

        // ==============================================================
        // 1. CONSTRUCTEUR / CHARGEMENT
        // ==============================================================

        [Fact]
        public void Ctor_Loads_Data_From_DALs()
        {
            // Arrange
            var station = new Station
            {
                Id = 1,
                Nom = "S1",
                CapaciteMaxTrains = 5,
            };
            var train = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
                StationId = 1,
                Station = station,
            };
            var block = new Block { Id = 1, Nom = "B1" };
            var point = new PointArret
            {
                Id = 1,
                Nom = "PA1",
                Latitude = 1,
                Longitude = 2,
            };

            // Act
            var vm = CreateViewModel(
                trains: new[] { train },
                stations: new[] { station },
                blocks: new[] { block },
                points: new[] { point }
            );

            // Assert
            Assert.Single(vm.Trains);
            Assert.Single(vm.Stations);
            Assert.Single(vm.Blocks);
            Assert.Single(vm.PointsInteret);
        }

        // ==============================================================
        // 2. ADD TRAIN
        // ==============================================================

        [Fact]
        public void AddTrain_WhenNoStationHasCapacity_ShowsMessage_AndDoesNotAddTrain()
        {
            // Arrange
            var station = new Station
            {
                Id = 1,
                Nom = "S1",
                CapaciteMaxTrains = 1,
            };
            var existingTrain = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
                StationId = 1,
                Station = station,
            };

            var vm = CreateViewModel(trains: new[] { existingTrain }, stations: new[] { station });

            Train dummyOutTrain = null!;
            _dialogMock
                .Setup(d => d.ShowTrainDialog(It.IsAny<List<Station>>(), out dummyOutTrain))
                .Returns(false)
                .Verifiable(); // ne devrait jamais être invoqué

            _dialogMock
                .Setup(d =>
                    d.ShowMessage(
                        It.Is<string>(m => m.Contains("Aucune station n’a de capacité disponible")),
                        It.IsAny<string>()
                    )
                )
                .Verifiable();

            // Act
            vm.AjouterTrainCommand.Execute(null);

            // Assert
            _dialogMock.Verify();
            _trainDalMock.Verify(d => d.AddTrain(It.IsAny<Train>()), Times.Never);
            Assert.Single(vm.Trains); // toujours 1 train
        }

        [Fact]
        public void AddTrain_WithAvailableStation_AddsTrain_AndCallsDAL()
        {
            // Arrange
            var station = new Station
            {
                Id = 1,
                Nom = "S1",
                CapaciteMaxTrains = 2,
            };
            var existingTrain = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
                StationId = 1,
                Station = station,
            };

            var vm = CreateViewModel(trains: new[] { existingTrain }, stations: new[] { station });

            var newTrain = new Train
            {
                Id = 2,
                Nom = "T2",
                Etat = EtatTrain.EnGare,
                StationId = station.Id,
                Station = station,
            };

            Train outTrain = newTrain;

            _dialogMock
                .Setup(d => d.ShowTrainDialog(It.IsAny<List<Station>>(), out outTrain))
                .Returns(true);

            _trainDalMock.Setup(d => d.AddTrain(newTrain));

            _dialogMock.Setup(d =>
                d.ShowMessage(
                    It.Is<string>(m => m.Contains("ajouté avec succès")),
                    It.IsAny<string>()
                )
            );

            // Act
            vm.AjouterTrainCommand.Execute(null);

            // Assert
            _trainDalMock.Verify(d => d.AddTrain(newTrain), Times.Once);
            Assert.Equal(2, vm.Trains.Count);
            Assert.Contains(vm.Trains, t => t.Nom == "T2");
        }

        // ==============================================================
        // 3. DELETE TRAIN
        // ==============================================================

        [Fact]
        public void DeleteTrain_WhenDialogReturnsTrain_RemovesTrain_AndCallsDAL()
        {
            // Arrange
            var station = new Station
            {
                Id = 1,
                Nom = "S1",
                CapaciteMaxTrains = 5,
            };
            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
                Station = station,
            };
            var t2 = new Train
            {
                Id = 2,
                Nom = "T2",
                Etat = EtatTrain.EnGare,
                Station = station,
            };

            var vm = CreateViewModel(trains: new[] { t1, t2 }, stations: new[] { station });

            Train outTrain = t1;

            _dialogMock
                .Setup(d => d.ShowDeleteTrainDialog(It.IsAny<List<Station>>(), out outTrain))
                .Returns(true);

            _trainDalMock.Setup(d => d.DeleteTrain(t1.Id));

            _dialogMock.Setup(d =>
                d.ShowMessage(
                    It.Is<string>(m => m.Contains("supprimé avec succès")),
                    It.IsAny<string>()
                )
            );

            // Act
            vm.SupprimerTrainCommand.Execute(null);

            // Assert
            _trainDalMock.Verify(d => d.DeleteTrain(t1.Id), Times.Once);
            Assert.Single(vm.Trains);
            Assert.DoesNotContain(vm.Trains, t => t.Id == 1);
        }

        // ==============================================================
        // 4. PLANIFICATION D’ITINÉRAIRE
        // ==============================================================

        [Fact]
        public void PlanifierItineraire_WhenNoTrainAvailable_ShowsMessage_AndDoesNotCallDialog()
        {
            // Arrange : tous les trains sont EnTransit ou HorsService
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
                Etat = EtatTrain.HorsService,
            };

            var vm = CreateViewModel(trains: new[] { t1, t2 });

            _dialogMock
                .Setup(d =>
                    d.ShowMessage(
                        It.Is<string>(m => m.Contains("Aucun train disponible")),
                        It.IsAny<string>()
                    )
                )
                .Verifiable();

            // Act
            vm.PlanifierItineraireCommand.Execute(null);

            // Assert
            _dialogMock.Verify();
            // aucun appel au dialog de planif
            Train outTrain = null!;
            List<PointArret> outArrets = null!;
            DateTime outDep = default,
                outArr = default;

            _dialogMock.Verify(
                d =>
                    d.ShowPlanifierItineraireDialog(
                        It.IsAny<List<Train>>(),
                        It.IsAny<List<PointArret>>(),
                        out outTrain,
                        out outArrets,
                        out outDep,
                        out outArr
                    ),
                Times.Never
            );
        }

        [Fact]
        public void PlanifierItineraire_WhenDialogCancelled_DoesNothing()
        {
            // Arrange
            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
            };

            var p1 = new PointArret
            {
                Id = 1,
                Nom = "A",
                Latitude = 1,
                Longitude = 1,
            };
            var p2 = new PointArret
            {
                Id = 2,
                Nom = "B",
                Latitude = 2,
                Longitude = 2,
            };

            var vm = CreateViewModel(trains: new[] { t1 }, points: new[] { p1, p2 });

            Train outTrain = null!;
            List<PointArret> outArrets = null!;
            DateTime outDep = default,
                outArr = default;

            _dialogMock
                .Setup(d =>
                    d.ShowPlanifierItineraireDialog(
                        It.IsAny<List<Train>>(),
                        It.IsAny<List<PointArret>>(),
                        out outTrain,
                        out outArrets,
                        out outDep,
                        out outArr
                    )
                )
                .Returns(false);

            _itineraireDalMock
                .Setup(d => d.PlanifierItineraire(It.IsAny<Itineraire>()))
                .Verifiable();

            // Act
            vm.PlanifierItineraireCommand.Execute(null);

            _itineraireDalMock.Verify(
                d => d.PlanifierItineraire(It.IsAny<Itineraire>()),
                Times.Never
            );
        }

        [Fact]
        public void PlanifierItineraire_WhenSecurityRulesFail_ShowsMessage_AndDoesNotPersistItinerary()
        {
            // Arrange
            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
            };

            var p1 = new PointArret
            {
                Id = 1,
                Nom = "A",
                Latitude = 1,
                Longitude = 1,
            };
            var p2 = new PointArret
            {
                Id = 2,
                Nom = "B",
                Latitude = 2,
                Longitude = 2,
            };

            var vm = CreateViewModel(
                trains: new[] { t1 },
                blocks: Array.Empty<Block>(),
                points: new[] { p1, p2 }
            );

            var arretsSelectionnes = new List<PointArret> { p1, p2 };
            var trainSelectionne = t1;
            var depart = DateTime.Now;
            var arrivee = depart.AddHours(1);

            Train outTrain = trainSelectionne;
            List<PointArret> outArrets = arretsSelectionnes;
            DateTime outDep = depart;
            DateTime outArr = arrivee;

            _dialogMock
                .Setup(d =>
                    d.ShowPlanifierItineraireDialog(
                        It.IsAny<List<Train>>(),
                        It.IsAny<List<PointArret>>(),
                        out outTrain,
                        out outArrets,
                        out outDep,
                        out outArr
                    )
                )
                .Returns(true);

            _dialogMock
                .Setup(d =>
                    d.ShowMessage(
                        It.Is<string>(m => m.Contains("viole les règles de sécurité")),
                        It.IsAny<string>()
                    )
                )
                .Verifiable();

            // Act
            vm.PlanifierItineraireCommand.Execute(null);

            // Assert
            _dialogMock.Verify();
            _itineraireDalMock.Verify(
                d => d.PlanifierItineraire(It.IsAny<Itineraire>()),
                Times.Never
            );
        }

        [Fact]
        public void PlanifierItineraire_HappyPath_PersistsItinerary_AndUpdatesTrainState()
        {
            // Arrange
            var station = new Station
            {
                Id = 1,
                Nom = "S1",
                CapaciteMaxTrains = 5,
            };
            var t1 = new Train
            {
                Id = 1,
                Nom = "T1",
                Etat = EtatTrain.EnGare,
                StationId = station.Id,
                Station = station,
            };

            var p1 = new PointArret
            {
                Id = 1,
                Nom = "Gare Québec-Gatineau",
                Latitude = 46.79,
                Longitude = -71.33,
            };
            var p2 = new PointArret
            {
                Id = 2,
                Nom = "Gare du palais",
                Latitude = 46.81,
                Longitude = -71.21,
            };

            // Un block approximatif entre ces 2 points
            var block = new Block
            {
                Id = 1,
                Nom = "Gare Québec-Gatineau → Gare du palais",
                LatitudeDepart = p1.Latitude,
                LongitudeDepart = p1.Longitude,
                LatitudeArrivee = p2.Latitude,
                LongitudeArrivee = p2.Longitude,
                EstOccupe = false,
                Signal = SignalType.Vert,
            };

            var vm = CreateViewModel(
                trains: new[] { t1 },
                stations: new[] { station },
                blocks: new[] { block },
                points: new[] { p1, p2 }
            );

            var arretsSelectionnes = new List<PointArret> { p1, p2 };
            var trainSelectionne = t1;
            var depart = DateTime.Now;
            var arrivee = depart.AddMinutes(30);

            Train outTrain = trainSelectionne;
            List<PointArret> outArrets = arretsSelectionnes;
            DateTime outDep = depart;
            DateTime outArr = arrivee;

            _dialogMock
                .Setup(d =>
                    d.ShowPlanifierItineraireDialog(
                        It.IsAny<List<Train>>(),
                        It.IsAny<List<PointArret>>(),
                        out outTrain,
                        out outArrets,
                        out outDep,
                        out outArr
                    )
                )
                .Returns(true);

            _itineraireDalMock
                .Setup(d => d.GetItineraireParTrain(t1.Id))
                .Returns((Itineraire)null!); // aucun itinéraire existant

            _itineraireDalMock
                .Setup(d => d.PlanifierItineraire(It.IsAny<Itineraire>()))
                .Verifiable();

            _trainDalMock
                .Setup(d =>
                    d.UpdateTrain(
                        It.Is<Train>(tr => tr.Id == t1.Id && tr.Etat == EtatTrain.Programme)
                    )
                )
                .Verifiable();

            _dialogMock
                .Setup(d =>
                    d.ShowMessage(
                        It.Is<string>(m => m.Contains("Itinéraire planifié avec succès")),
                        It.IsAny<string>()
                    )
                )
                .Verifiable();

            // Act
            vm.PlanifierItineraireCommand.Execute(null);

            // Assert
            _itineraireDalMock.Verify(
                d => d.PlanifierItineraire(It.IsAny<Itineraire>()),
                Times.Once
            );
            _trainDalMock.Verify();
            _dialogMock.Verify();
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
