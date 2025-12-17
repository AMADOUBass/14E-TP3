using System.Collections.ObjectModel;
using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.ViewModel
{
    public class EmployeDashboardViewModel : BaseViewModel
    {
        private readonly IStationDAL _stationDAL;
        private readonly ITrainDAL _trainDAL;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger _logger;

        private Station? _stationAssignee;
        public Station? StationAssignee
        {
            get => _stationAssignee;
            private set
            {
                if (SetProperty(ref _stationAssignee, value))
                {
                    OnPropertyChanged(nameof(StationNomAffiche));
                }
            }
        }

        public string StationNomAffiche => StationAssignee?.Nom ?? "Aucune station assignée";

        public ObservableCollection<Train> TrainsDeLaStation { get; private set; } = new();

        public ObservableCollection<Train> TrainsEnGare =>
            new(TrainsDeLaStation.Where(t => t.Etat == EtatTrain.EnGare));

        /**
         * Constructeur du HomeViewModel.
         *@param stationDAL Le service d'accès aux données des stations.
         *@param trainDAL Le service d'accès aux données des trains.
         *@param userSessionService Le service de gestion de la session utilisateur.
         */
        public EmployeDashboardViewModel(
            IStationDAL stationDAL,
            ITrainDAL trainDAL,
            IUserSessionService userSessionService,
            ILogger logger
        )
        {
            _stationDAL = stationDAL;
            _trainDAL = trainDAL;
            _userSessionService = userSessionService;
            _logger = logger;

            _userSessionService.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IUserSessionService.ConnectedUser))
                    ChargerStation();
            };

            ChargerStation();
        }

        /* * Charge la station assignée à l'utilisateur connecté
         * et les trains associés à cette station.
         */
        private void ChargerStation()
        {
            var user = _userSessionService.ConnectedUser;

            StationAssignee = user switch
            {
                null => new Station { Nom = "Aucun utilisateur connecté" },
                { StationId: null } => new Station { Nom = "Aucune station assignée" },
                _ => _stationDAL.GetStationById(user.StationId.Value)
                    ?? new Station { Nom = "Station introuvable" },
            };

            ChargerTrains();
            _logger.Info(
                $"Station assignée chargée pour l'utilisateur '{user?.Username ?? "Employe"}'."
            );
        }

        /* * Charge les trains associés à la station assignée.
         */
        private void ChargerTrains()
        {
            if (StationAssignee?.Id is not > 0)
                return;

            var trains = _trainDAL
                .GetAllTrains()
                .Where(t => t.StationId == StationAssignee.Id)
                .ToList();

            TrainsDeLaStation = new ObservableCollection<Train>(trains);

            OnPropertyChanged(nameof(TrainsDeLaStation));
            OnPropertyChanged(nameof(TrainsEnGare));

            _logger.Info(
                $"{TrainsDeLaStation.Count} trains chargés pour la station '{StationAssignee.Nom}'."
            );
        }
    }
}
