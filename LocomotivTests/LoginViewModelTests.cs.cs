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

        private MainViewModel CréerVueModèle(bool estConnecté = true)
        {
            _sessionMock.Setup(s => s.IsUserConnected).Returns(estConnecté);
            return new MainViewModel(_navMock.Object, _sessionMock.Object);
        }

        [Fact]
        public void Constructeur_DoitNaviguerVersVueAccueil()
        {
            var vueModèle = CréerVueModèle();
            _navMock.Verify(n => n.NavigateTo<HomeViewModel>(), Times.Once);
        }

        [Fact]
        public void CommandeNavigationVersConnexion_DoitDéclencherNavigation()
        {
            var vueModèle = CréerVueModèle();
            vueModèle.NavigateToConnectUserViewCommand.Execute(null);
            _navMock.Verify(n => n.NavigateTo<LoginViewModel>(), Times.Once);
        }

        [Fact]
        public void CommandeDéconnexion_DoitRéinitialiserSessionEtNaviguer()
        {
            var vm = CréerVueModèle();
            vm.DisconnectCommand.Execute(null);

            _sessionMock.VerifySet(s => s.ConnectedUser = null);
            _navMock.Verify(n => n.NavigateTo<LoginViewModel>(), Times.Once);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void PeutSeDéconnecter_ReflèteÉtatConnexion(bool estConnecté, bool peutExécuterAttendu)
        {
            var vm = CréerVueModèle(estConnecté);
            var peutExécuter = vm.DisconnectCommand.CanExecute(null);
            Assert.Equal(peutExécuterAttendu, peutExécuter);
        }
    }
}