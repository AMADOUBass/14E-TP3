using Locomotiv.Data;
using Locomotiv.Model;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;
using Xunit;
using static User;

namespace LocomotivTests
{
    public class HomeViewModelTests
    {
        private readonly Mock<IUserDAL> _userDalMock = new();
        private readonly Mock<ITrainDAL> _trainDalMock = new();
        private readonly Mock<IStationDAL> _stationDalMock = new();
        private readonly Mock<IBlockDAL> _blockDalMock = new();
        private readonly Mock<INavigationService> _navMock = new();
        private readonly Mock<IUserSessionService> _sessionMock = new();
        private readonly Mock<IDialogService> _dialogMock = new();
        private readonly Mock<IPointArretDAL> _pointArretDalMock = new();
        private readonly Mock<IItineraireDAL> _itineraireDalMock = new();

        private HomeViewModel CreateViewModel(User? user = null)
        {
            _sessionMock.SetupGet(s => s.ConnectedUser).Returns(user);
            _sessionMock.SetupGet(s => s.IsUserConnected).Returns(user != null);

            return new HomeViewModel(
                _userDalMock.Object,
                _navMock.Object,
                _sessionMock.Object,
                _dialogMock.Object,
                _trainDalMock.Object,
                _stationDalMock.Object,
                _blockDalMock.Object,
                _pointArretDalMock.Object,
                _itineraireDalMock.Object
               

            );
        }

        [Fact]
        public void Message_De_Bienvenue_QuandUtilisateurNonConnecte_RetourneMessageGenerique()
        {
            var vm = CreateViewModel(null);
            Assert.Equal("Bienvenue sur Locomotiv Quebec Veuillez vous connecter", vm.WelcomeMessage);
        }

        [Fact]
        public void Message_De_Bienvenue_QuandUtilisateurAucunPrenom_RetourneNomSeulement()
        {
            var user = new User { Nom = "Diallo", Prenom = null };
            var vm = CreateViewModel(user);
            Assert.Equal("Bienvenue, Diallo !", vm.WelcomeMessage);
        }

        [Fact]
        public void Message_De_Bienvenue_QuandUtilisateurAucunNom_RetournePrenomSeulement()
        {
            var user = new User { Nom = "Diallo", Prenom = "Amadou" };
            var vm = CreateViewModel(user);
            Assert.Equal("Bienvenue, Amadou Diallo !", vm.WelcomeMessage);
        }

        [Theory]
        [InlineData(UserRole.Admin, true, false)]
        [InlineData(UserRole.Employe, false, true)]
        public void Les_indicateurs_de_role_reflechissent_le_role_de_l_utilisateur(UserRole role, bool expectAdmin, bool expectEmploye)
        {
            var user = new User { Role = role };
            _sessionMock.SetupGet(s => s.ConnectedUser).Returns(user);
            _sessionMock.SetupGet(s => s.IsUserConnected).Returns(true);

            var vm = new HomeViewModel(
                _userDalMock.Object,
                _navMock.Object,
                _sessionMock.Object,
                _dialogMock.Object,
                _trainDalMock.Object,
                _stationDalMock.Object,
                _blockDalMock.Object,
                _pointArretDalMock.Object,
                _itineraireDalMock.Object
            );

            Assert.Equal(expectAdmin, vm.IsAdmin);
            Assert.Equal(expectEmploye, vm.IsEmploye);
        }

        [Fact]
        public void CommandeDeDeconnexion_ReinitialiseSessionEtNavigue()
        {
            var user = new User { Nom = "Diallo" };
            var vm = CreateViewModel(user);

            vm.LogoutCommand.Execute(null);

            _sessionMock.VerifySet(s => s.ConnectedUser = null);
            _navMock.Verify(n => n.NavigateTo<LoginViewModel>(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PeutSeDeconnecter_ReflecteEtatDeConnexion(bool isConnected)
        {
            var user = isConnected ? new User() : null;
            var vm = CreateViewModel(user);
            var canExecute = vm.LogoutCommand.CanExecute(null);
            Assert.Equal(isConnected, canExecute);
        }
    }
}