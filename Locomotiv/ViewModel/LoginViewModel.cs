using Locomotiv.Model;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static User;

namespace Locomotiv.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserDAL _userDAL;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IDialogService _dialogService;

        public ICommand LoginCommand { get; }

        public LoginViewModel(IUserDAL userDAL, INavigationService navigationService, IUserSessionService userSessionService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            _userDAL = userDAL;
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            LoginCommand = new RelayCommand(Login, CanLogin);

        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                    ValidateProperty(nameof(Username), value);
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                    ValidateProperty(nameof(Password), value);
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private void Login()
        {
            IsBusy = true;
            ClearErrors(nameof(Password));

            try
            {
                var user = _userDAL.FindByUsernameAndPassword(Username, Password);
                if (user != null)
                {
                    _userSessionService.ConnectedUser = user;

                    _dialogService.ShowMessage($"Bienvenue, {user.Prenom} {user.Nom}!", "Connexion réussie");

                    if (user.Role == UserRole.Admin)
                        _navigationService.NavigateTo<HomeViewModel>();
                    else
                        _navigationService.NavigateTo<HomeViewModel>();
                }
                else
                {
                    AddError(nameof(Password), "Utilisateur ou mot de passe invalide.");

                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Une erreur est survenue lors de la tentative de connexion : {ex.Message}", "Erreur");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ErrorMessages));
            }
        }

        private bool CanLogin()
        {
            return !HasErrors && Username.NotEmpty() && Password.NotEmpty();
        }

        private void ValidateProperty(string propertyName, string value)
        {
            ClearErrors(propertyName);

            switch (propertyName)
            {
                case nameof(Username):
                    if (value.Empty())
                        AddError(propertyName, "Le nom d'utilisateur est requis.");
                    else if (value.Length < 2)
                        AddError(propertyName, "Le nom d'utilisateur doit contenir au moins 2 caractères.");
                    break;

                case nameof(Password):
                    if (value.Empty())
                        AddError(propertyName, "Le mot de passe est requis.");
                    break;
            }

            OnPropertyChanged(nameof(ErrorMessages));
        }
    }
}