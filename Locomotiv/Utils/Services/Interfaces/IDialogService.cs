using Locomotiv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "Info");


        bool ShowTrainDialog(List<Station> stations, out Train train);

        bool ShowPlanifierItineraireDialog(List<Train> Train, List<PointArret> pointsArret, out Train trainSélectionné, out List<PointArret> arretsSélectionnés);

        bool ShowDeleteTrainDialog(List<Station> stations, out Train train);
    }

}
