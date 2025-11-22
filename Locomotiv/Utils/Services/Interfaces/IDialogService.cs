using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Locomotiv.Model;
using Locomotiv.View;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "Info");

        bool ShowTrainDialog(List<Station> stations, out Train train);

        public bool ShowPlanifierItineraireDialog(
            List<Train> trainsDisponibles,
            List<PointArret> pointsArretDisponibles,
            out Train trainSelectionne,
            out List<PointArret> arretsSelectionnes,
            out DateTime dateDepart,
            out DateTime dateArrivee
        )
        {
            var dialog = new PlanifierItineraireDialog(trainsDisponibles, pointsArretDisponibles);

            // Résultat du dialog : true si l'utilisateur valide, false s'il annule ou ferme
            var result = dialog.ShowDialog() == true;

            if (result)
            {
                // ✅ L'utilisateur a validé → on récupère les vraies valeurs
                trainSelectionne = dialog.TrainSelectionne;
                arretsSelectionnes = dialog.ArretsSelectionnes ?? new List<PointArret>();
                dateDepart = dialog.DateDepart;
                dateArrivee = dialog.DateArrivee;

                // 🔒 Validation basique des données
                if (trainSelectionne == null || dateArrivee <= dateDepart)
                {
                    // Si données incohérentes → on force un retour "false"
                    trainSelectionne = null;
                    arretsSelectionnes = new List<PointArret>();
                    dateDepart = DateTime.MinValue;
                    dateArrivee = DateTime.MinValue;
                    return false;
                }
            }
            else
            {
                // ❌ L'utilisateur a annulé → valeurs par défaut pour éviter NullReference
                trainSelectionne = null;
                arretsSelectionnes = new List<PointArret>();
                dateDepart = DateTime.MinValue;
                dateArrivee = DateTime.MinValue;
            }

            return result;
        }

        bool ShowDeleteTrainDialog(List<Station> stations, out Train train);
    }
}
