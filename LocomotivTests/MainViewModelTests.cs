using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;

namespace LocomotivTests
{
    public class LoginViewModelTests
    {
        private readonly Mock<IUserDAL> _userDalMock = new();
        private readonly Mock<INavigationService> _navMock = new();
        private readonly Mock<IUserSessionService> _sessionMock = new();
        private readonly Mock<IDialogService> _dialogMock = new();
        private readonly Mock<ILogger> _loggerMock = new();

        private LoginViewModel CréerVueModèle() =>
            new LoginViewModel(
                _userDalMock.Object,
                _navMock.Object,
                _sessionMock.Object,
                _dialogMock.Object,
                _loggerMock.Object
            );

        [Fact]
        public void Connexion_IdentifiantsValides_DefinitUtilisateurEtNavigue()
        {
            var user = new User
            {
                Username = "amadou",
                Prenom = "Amadou",
                Nom = "Diallo",
                Role = UserRole.Admin,
            };
            _userDalMock
                .Setup(d => d.FindByUsernameAndPassword("amadou", "secure123"))
                .Returns(user);

            var vm = CréerVueModèle();
            vm.Username = "amadou";
            vm.Password = "secure123";

            vm.LoginCommand.Execute(null);

            _sessionMock.VerifySet(s => s.ConnectedUser = user);
            _dialogMock.Verify(
                d => d.ShowMessage("Bienvenue, Amadou Diallo!", "Connexion réussie"),
                Times.Once
            );
            _navMock.Verify(n => n.NavigateTo<HomeViewModel>());
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public void Connexion_IdentifiantsInvalides_AjouteMessageErreur()
        {
            _userDalMock
                .Setup(d => d.FindByUsernameAndPassword("wronguser", "wrongpass"))
                .Returns((User?)null);

            var vm = CréerVueModèle();
            vm.Username = "wronguser";
            vm.Password = "wrongpass";

            vm.LoginCommand.Execute(null);

            Assert.True(vm.HasErrors);
            Assert.Contains("Utilisateur ou mot de passe invalide.", vm.ErrorMessages);
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public void Connexion_ExceptionAffichée_AfficheDialogueErreur()
        {
            _userDalMock
                .Setup(d => d.FindByUsernameAndPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("DB down"));

            var vm = CréerVueModèle();
            vm.Username = "amadou";
            vm.Password = "secure123";

            vm.LoginCommand.Execute(null);

            _dialogMock.Verify(d =>
                d.ShowMessage(It.Is<string>(msg => msg.Contains("DB down")), "Erreur")
            );
            Assert.False(vm.IsBusy);
        }

        [Theory]
        [InlineData("", "secure123", true)]
        [InlineData("a", "secure123", true)]
        [InlineData("amadou", "", true)]
        [InlineData("amadou", "secure123", false)]
        public void PeutSeConnecter_ReflèteÉtatValidation(
            string nomUtilisateur,
            string motDePasse,
            bool erreurAttendue
        )
        {
            var vm = CréerVueModèle();
            vm.Username = nomUtilisateur;
            vm.Password = motDePasse;

            var peutSeConnecter = vm.LoginCommand.CanExecute(null);

            Assert.Equal(!erreurAttendue, peutSeConnecter);
        }
    }
}
