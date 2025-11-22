using System.Windows;
using Locomotiv.Model;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.View;

namespace Locomotiv.Utils.Services
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message, string title = "Info")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowTrainDialog(List<Station> stations, out Train train)
        {
            var dialog = new TrainFormDialog(stations);
            var result = dialog.ShowDialog();
            if (result == true)
            {
                train = dialog.Train;
                return true;
            }
            train = null;
            return false;
        }

        public bool ShowPlanifierItineraireDialog(
            List<Train> Train,
            List<PointArret> pointsArret,
            out Train trainSélectionné,
            out List<PointArret> arretsSélectionnés
        )
        {
            var dialog = new PlanifierItineraireDialog(Train, pointsArret);
            var result = dialog.ShowDialog() == true;

            trainSélectionné = dialog.TrainSelectionne;
            arretsSélectionnés = dialog.ArretsSelectionnes;

            return result;
        }

        public bool ShowDeleteTrainDialog(List<Station> stations, out Train train)
        {
            var dialog = new DeleteTrainDialog(stations);
            var result = dialog.ShowDialog();
            train = dialog.TrainASupprimer;
            return result == true && train != null;
        }
    }
}
