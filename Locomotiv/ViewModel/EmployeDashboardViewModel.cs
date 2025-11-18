using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.ViewModel
{
    public class EmployeDashboardViewModel : BaseViewModel
    {
        public readonly IStationDAL _stationDAL;
        private readonly IUserSessionService _userSessionService;


        private Station? _stationAssignee;
        public Station? StationAssignee
        {
            get => _stationAssignee;
            set
            {
                _stationAssignee = value;
                OnPropertyChanged();
            }
        }



        public EmployeDashboardViewModel(IStationDAL stationDAL, IUserSessionService userSessionService)
        {
            _stationDAL = stationDAL;
            _userSessionService = userSessionService;
            ChargerStationAssignee();
        }
        private void ChargerStationAssignee()
        {
            //var user = _userSessionService.ConnectedUser;
            //if (user?.StationId == null)
            //{
            //    StationAssignee = new Station { Nom = "Aucune station assignée" };
            //    return;
            //}

            //StationAssignee = _stationDAL.getOnlyStationBYId(1);
        }

    }
}
