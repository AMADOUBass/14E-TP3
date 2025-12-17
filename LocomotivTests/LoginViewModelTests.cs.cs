using Locomotiv.Data;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LocomotivTests
{
    public class MainViewModelTests
    {
        private readonly Mock<INavigationService> _navMock = new();
        private readonly Mock<IUserSessionService> _sessionMock = new();
        private readonly Mock<ILogger> _loggerMock = new();

        private MainViewModel CreerVueModele(bool estConnecte = true)
        {
            _sessionMock.Setup(s => s.IsUserConnected).Returns(estConnecte);
            return new MainViewModel(_navMock.Object, _sessionMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructeur_DoitNaviguerVersVueAccueil()
        {
            var vueModele = CreerVueModele();
            _navMock.Verify(n => n.NavigateTo<HomeViewModel>(), Times.Once);
        }

        [Fact]
        public void CommandeNavigationVersConnexion_DoitDeclencherNavigation()
        {
            var vueModele = CreerVueModele();
            vueModele.NavigateToConnectUserViewCommand.Execute(null);
            _navMock.Verify(n => n.NavigateTo<LoginViewModel>(), Times.Once);
        }

        [Fact]
        public void CommandeDeconnexion_DoitReinitialiserSessionEtNaviguer()
        {
            var vm = CreerVueModele();
            vm.DisconnectCommand.Execute(null);

            _sessionMock.VerifySet(s => s.ConnectedUser = null);
            _navMock.Verify(n => n.NavigateTo<LoginViewModel>(), Times.Once);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void PeutSeDeconnecter_RefleteEtatConnexion(
            bool estConnecte,
            bool peutExecuterAttendu
        )
        {
            var vm = CreerVueModele(estConnecte);
            var peutExecuter = vm.DisconnectCommand.CanExecute(null);
            Assert.Equal(peutExecuterAttendu, peutExecuter);
        }
    }
}
