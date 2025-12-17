using System.Windows.Input;
using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly IUserDAL _userDAL;
        private readonly ITrainDAL _trainDAL;
        private readonly IStationDAL _stationDAL;
        private readonly IBlockDAL _blockDAL;
        private readonly IPointArretDAL _pointArretDAL;
        private readonly IItineraireDAL _itineraireDAL;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IItineraireService _itineraireService;

        private readonly IDialogService _dialogService;
        public AdminDashboardViewModel AdminDashboardVM { get; }
        public EmployeDashboardViewModel EmployeeDashboardVM { get; }
        public ClientComDashboardViewModel ClientComDashboardVM { get; }
        public ClientDashboardViewModel ClientDashboardVM { get; }

        public User? ConnectedUser
        {
            get => _userSessionService.ConnectedUser;
        }

        public string WelcomeMessage
        {
            get =>
                ConnectedUser == null ? "Bienvenue sur Locomotiv Quebec Veuillez vous connecter"
                : ConnectedUser.Prenom == null ? $"Bienvenue, {ConnectedUser.Nom} !"
                : $"Bienvenue, {ConnectedUser.Prenom} {ConnectedUser.Nom} !";
        }

        public bool IsAdmin => ConnectedUser?.Role == UserRole.Admin;
        public bool IsEmploye => ConnectedUser?.Role == UserRole.Employe;
        public bool IsClientCom => ConnectedUser?.Role == UserRole.ClientCommercial;
        public bool IsClient => ConnectedUser?.Role == UserRole.Client;

        public ICommand LogoutCommand { get; set; }

        /**
         * Constructeur du HomeViewModel.
         *
         * @param userDAL Le service d'accès aux données utilisateur.
         * @param navigationService Le service de navigation.
         * @param userSessionService Le service de gestion de la session utilisateur.
         * @param dialogService Le service de dialogue pour afficher des messages.
         * @param trainDAL Le service d'accès aux données des trains.
         * @param stationDAL Le service d'accès aux données des stations.
         * @param blockDAL Le service d'accès aux données des blocs.
         * @param pointArretDAL Le service d'accès aux données des points d'arrêt.
         * @param itineraireDAL Le service d'accès aux données des itinéraires.
         */
        public HomeViewModel(
            IUserDAL userDAL,
            INavigationService navigationService,
            IUserSessionService userSessionService,
            IDialogService dialogService,
            ITrainDAL trainDAL,
            IStationDAL stationDAL,
            IBlockDAL blockDAL,
            IPointArretDAL pointArretDAL,
            IItineraireDAL itineraireDAL,
            IItineraireService itineraireService
        )
        {
            _userDAL = userDAL;
            _trainDAL = trainDAL;
            _stationDAL = stationDAL;
            _blockDAL = blockDAL;
            _pointArretDAL = pointArretDAL;
            _itineraireDAL = itineraireDAL;
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _itineraireService = itineraireService;
            _dialogService = dialogService;
            LogoutCommand = new RelayCommand(Logout, CanLogout);
            AdminDashboardVM = new AdminDashboardViewModel(
                trainDAL,
                dialogService,
                stationDAL,
                _blockDAL,
                pointArretDAL,
                itineraireDAL
            );
            EmployeeDashboardVM = new EmployeDashboardViewModel(
                stationDAL,
                trainDAL,
                userSessionService
            );
            ClientComDashboardVM = new ClientComDashboardViewModel(
                //userDAL,
                //trainDAL,
                //stationDAL,
                //itineraireDAL,
                //dialogService
                userSessionService
            );
            ClientDashboardVM = new ClientDashboardViewModel(
                //trainDAL,
                //stationDAL,
                //itineraireDAL,
                //dialogService,
                //userSessionService
                userSessionService,
                itineraireService
                
            );
        }

        /*  * Méthode pour déconnecter l'utilisateur.
         */
        private void Logout()
        {
            _userSessionService.ConnectedUser = null;
            (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsEmploye));
            OnPropertyChanged(nameof(IsClientCom));
            OnPropertyChanged(nameof(IsClient));
            _navigationService.NavigateTo<LoginViewModel>();
        }

        /*  * Méthode pour vérifier si l'utilisateur peut se déconnecter.
         */
        private bool CanLogout()
        {
            return _userSessionService.IsUserConnected;
        }
    }
}
