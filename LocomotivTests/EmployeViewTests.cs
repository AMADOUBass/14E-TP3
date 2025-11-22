using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;
using Xunit;

namespace LocomotivTests
{
    public class EmployeDashboardViewModelTests
    {
        private readonly Mock<IStationDAL> _stationDalMock = new();
        private readonly Mock<ITrainDAL> _trainDalMock = new();
        private readonly FakeSessionService _session = new();

        private EmployeDashboardViewModel CreerVueModele()
        {
            return new EmployeDashboardViewModel(
                _stationDalMock.Object,
                _trainDalMock.Object,
                _session
            );
        }

        [Fact]
        public void StationAssignee_NullUser_RetourneMessageUtilisateurNonConnecte()
        {
            _session.ConnectedUser = null;
            var vm = CreerVueModele();

            Assert.Equal("Aucun utilisateur connecté", vm.StationAssignee?.Nom);
            Assert.Equal("Aucun utilisateur connecté", vm.StationNomAffiche);
        }

        [Fact]
        public void StationAssignee_UserSansStation_RetourneMessageAucuneStation()
        {
            _session.ConnectedUser = new User { StationId = null };
            var vm = CreerVueModele();

            Assert.Equal("Aucune station assignée", vm.StationAssignee?.Nom);
            Assert.Equal("Aucune station assignée", vm.StationNomAffiche);
        }

        [Fact]
        public void StationAssignee_StationIntrouvable_RetourneMessageStationIntrouvable()
        {
            _session.ConnectedUser = new User { StationId = 99 };
            _stationDalMock.Setup(s => s.GetStationById(99)).Returns((Station?)null);

            var vm = CreerVueModele();

            Assert.Equal("Station introuvable", vm.StationAssignee?.Nom);
        }

        [Fact]
        public void StationAssignee_StationValide_RetourneNomCorrect()
        {
            var station = new Station { Id = 1, Nom = "Gare Centrale" };
            _session.ConnectedUser = new User { StationId = 1 };
            _stationDalMock.Setup(s => s.GetStationById(1)).Returns(station);

            var vm = CreerVueModele();

            Assert.Equal("Gare Centrale", vm.StationAssignee?.Nom);
            Assert.Equal("Gare Centrale", vm.StationNomAffiche);
        }

        [Fact]
        public void TrainsDeLaStation_ChargeSeulementCeuxDeLaBonneStation()
        {
            var station = new Station { Id = 1, Nom = "Gare Centrale" };
            _session.ConnectedUser = new User { StationId = 1 };
            _stationDalMock.Setup(s => s.GetStationById(1)).Returns(station);

            var trains = new List<Train>
            {
                new Train { Nom = "T1", StationId = 1 },
                new Train { Nom = "T2", StationId = 2 },
            };
            _trainDalMock.Setup(t => t.GetAllTrains()).Returns(trains);

            var vm = CreerVueModele();

            Assert.Single(vm.TrainsDeLaStation);
            Assert.Equal("T1", vm.TrainsDeLaStation.First().Nom);
        }

        [Fact]
        public void TrainsEnGare_FiltreCorrectement()
        {
            var station = new Station { Id = 1, Nom = "Gare Centrale" };
            _session.ConnectedUser = new User { StationId = 1 };
            _stationDalMock.Setup(s => s.GetStationById(1)).Returns(station);

            var trains = new List<Train>
            {
                new Train
                {
                    Nom = "T1",
                    StationId = 1,
                    Etat = EtatTrain.EnGare,
                },
                new Train
                {
                    Nom = "T2",
                    StationId = 1,
                    Etat = EtatTrain.EnTransit,
                },
            };
            _trainDalMock.Setup(t => t.GetAllTrains()).Returns(trains);

            var vm = CreerVueModele();

            Assert.Single(vm.TrainsEnGare);
            Assert.Equal("T1", vm.TrainsEnGare.First().Nom);
        }

        [Fact]
        public void ChangementUtilisateur_DoitRechargerStation()
        {
            var station = new Station { Id = 1, Nom = "Gare Centrale" };
            _stationDalMock.Setup(s => s.GetStationById(1)).Returns(station);

            var vm = CreerVueModele();

            _session.ConnectedUser = new User { StationId = 1 };
            _session.NotifyPropertyChanged(nameof(_session.ConnectedUser));

            Assert.Equal("Gare Centrale", vm.StationAssignee?.Nom);
        }
    }

    public class FakeSessionService : IUserSessionService, INotifyPropertyChanged
    {
        private User? _connectedUser;
        public User? ConnectedUser
        {
            get => _connectedUser;
            set
            {
                _connectedUser = value;
                NotifyPropertyChanged(nameof(ConnectedUser));
            }
        }

        public bool IsUserConnected => ConnectedUser != null;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
