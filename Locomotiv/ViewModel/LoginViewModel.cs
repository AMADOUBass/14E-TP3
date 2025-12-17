using System.Windows.Input;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Commands;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserDAL _userDAL;
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;

        public ICommand LoginCommand { get; }

        /**
         * Constructeur du LoginViewModel.
         *
         * @param userDAL Le service d'accès aux données utilisateur.
         * @param navigationService Le service de navigation.
         * @param userSessionService Le service de gestion de la session utilisateur.
         * @param dialogService Le service de dialogue pour afficher des messages.
         */
        public LoginViewModel(
            IUserDAL userDAL,
            INavigationService navigationService,
            IUserSessionService userSessionService,
            IDialogService dialogService,
            ILogger logger
        )
        {
            _dialogService = dialogService;
            _userDAL = userDAL;
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _logger = logger;

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

        /**
         * Permet de tenter une connexion avec les informations fournies.
         */
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
                    _dialogService.ShowMessage(
                        $"Bienvenue, {user.Prenom} {user.Nom}!",
                        "Connexion réussie"
                    );
                    _navigationService.NavigateTo<HomeViewModel>();
                    _logger.Info(
                        $"L'utilisateur '{Username}' s'est connecté avec succès."
                    );
                }
                else
                {
                    AddError(nameof(Password), "Utilisateur ou mot de passe invalide.");
                    _logger.Warning(
                        $"Échec de la connexion pour l'utilisateur '{Username}'."
                    );
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(
                    $"Une erreur est survenue lors de la tentative de connexion : {ex.Message}",
                    "Erreur"
                );
                _logger.Error("Erreur lors de la connexion de l'utilisateur.", ex);
            }
            finally
            {

                IsBusy = false;
                OnPropertyChanged(nameof(ErrorMessages));
            }
        }

        /**
         * Vérifie si la commande de connexion peut être exécutée.
         *
         * @return true si la connexion peut être tentée, false sinon.
         */
        private bool CanLogin()
        {
            return !HasErrors && Username.NotEmpty() && Password.NotEmpty();
        }

        /**
         * Valide une propriété spécifique et ajoute des erreurs si nécessaire.
         *
         * @param propertyName Le nom de la propriété à valider.
         * @param value La valeur de la propriété à valider.
         */
        private void ValidateProperty(string propertyName, string value)
        {
            ClearErrors(propertyName);

            switch (propertyName)
            {
                case nameof(Username):
                    if (value.Empty())
                        AddError(propertyName, "Le nom d'utilisateur est requis.");
                    else if (value.Length < 2)
                        AddError(
                            propertyName,
                            "Le nom d'utilisateur doit contenir au moins 2 caractères."
                        );
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
