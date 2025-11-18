using Locomotiv.Model;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static User;

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

        private readonly IDialogService _dialogService;
        public AdminDashboardViewModel AdminDashboardVM { get; } 
        public EmployeDashboardViewModel EmployeeDashboardVM { get; } 


        public User? ConnectedUser
        {
            get => _userSessionService.ConnectedUser;
        }

        public string WelcomeMessage
        {
            get => ConnectedUser == null ? "Bienvenue sur Locomotiv Quebec Veuillez vous connecter" : ConnectedUser.Prenom == null ?
                $"Bienvenue, {ConnectedUser.Nom} !" : $"Bienvenue, {ConnectedUser.Prenom} {ConnectedUser.Nom} !";

        }

        public bool IsAdmin => ConnectedUser?.Role == UserRole.Admin;
        public bool IsEmploye => ConnectedUser?.Role == UserRole.Employe;
        // Commande pour la déconnexion
        public ICommand LogoutCommand { get; set; }

        public HomeViewModel(IUserDAL userDAL, INavigationService navigationService, IUserSessionService userSessionService, IDialogService dialogService, ITrainDAL trainDAL,IStationDAL stationDAL, IBlockDAL blockDAL , IPointArretDAL pointArretDAL, IItineraireDAL itineraireDAL)
        {
            _userDAL = userDAL;
            _trainDAL = trainDAL;
            _stationDAL = stationDAL;
            _blockDAL = blockDAL;
            _pointArretDAL = pointArretDAL;
            _itineraireDAL = itineraireDAL;
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _dialogService = dialogService;
            LogoutCommand = new RelayCommand(Logout, CanLogout);
            AdminDashboardVM = new AdminDashboardViewModel(trainDAL,dialogService,stationDAL, _blockDAL , pointArretDAL , itineraireDAL);
            EmployeeDashboardVM = new EmployeDashboardViewModel(stationDAL,userSessionService);

          

        }



        // Méthode pour gérer la déconnexion de l'utilisateur
        private void Logout()
        {
            _userSessionService.ConnectedUser = null;
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsEmploye));
            _navigationService.NavigateTo<LoginViewModel>();
        }

        // Vérifie si la commande de déconnexion peut être exécutée
        private bool CanLogout()
        {
            return _userSessionService.IsUserConnected;
        }
      
    }
}
