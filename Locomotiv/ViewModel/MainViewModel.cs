using System.Windows.Input;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger _logger;

        public INavigationService NavigationService
        {
            get => _navigationService;
        }

        public IUserSessionService UserSessionService
        {
            get => _userSessionService;
        }

        public ICommand NavigateToConnectUserViewCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand DisconnectCommand { get; }

        public bool IsUserConnected => _userSessionService.IsUserConnected;

        /**
         * Constructeur du MainViewModel.
         *
         * @param navigationService Le service de navigation.
         * @param userSessionService Le service de gestion de la session utilisateur.
         */
        public MainViewModel(
            INavigationService navigationService,
            IUserSessionService userSessionService,
            ILogger logger
        )
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _logger = logger;

            NavigateToConnectUserViewCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<LoginViewModel>()
            );
            NavigateToHomeCommand = new RelayCommand(() =>
                _navigationService.NavigateTo<HomeViewModel>()
            );
            DisconnectCommand = new RelayCommand(Disconnect, () => IsUserConnected);

            _navigationService.NavigateTo<HomeViewModel>();


        }

        /**
         * Permet de déconnecter l'utilisateur courant et de naviguer vers la vue de connexion.
         */
        private void Disconnect()
        {
            _userSessionService.ConnectedUser = null;
            OnPropertyChanged(nameof(IsUserConnected));
            _navigationService.NavigateTo<LoginViewModel>();
            _logger.Info("L'utilisateur s'est déconnecté.");
        }
    }
}
