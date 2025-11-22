using System.Windows;
using Locomotiv.Model;
using Locomotiv.Model.enums;

namespace Locomotiv.View
{
    /// <summary>
    /// Fenêtre de planification d’un itinéraire :
    /// - choix d’un train
    /// - choix et ordre des arrêts (stations / points d’intérêt)
    /// </summary>
    public partial class PlanifierItineraireDialog : Window
    {
        public DateTime DateDepart { get; private set; }
        public DateTime DateArrivee { get; private set; }

        public Train TrainSelectionne { get; private set; } = null!;
        public List<PointArret> ArretsSelectionnes { get; private set; } = new();

        private readonly List<PointArret> _etapes = new();

        public PlanifierItineraireDialog(List<Train> trains, List<PointArret> pointsArret)
        {
            InitializeComponent();

            var trainsDispo = trains.Where(t => t.Etat != EtatTrain.EnTransit).ToList();

            cmbTrain.ItemsSource = trainsDispo;
            lstPointsDisponibles.ItemsSource = pointsArret;
            lstItineraire.ItemsSource = _etapes;

            DateDepart = DateTime.Now.AddMinutes(2);
            DateArrivee = DateDepart.AddHours(1);
            if (!trainsDispo.Any())
            {
                MessageBox.Show(
                    "Aucun train disponible pour planifier un itinéraire. (tous sont en transit).",
                    "Planification d'itinéraire impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Close();
            }
        }

        private void BtnAjouterEtape_Click(object sender, RoutedEventArgs e)
        {
            if (lstPointsDisponibles.SelectedItem is not PointArret arret)
                return;

            if (!_etapes.Contains(arret))
            {
                _etapes.Add(arret);
                lstItineraire.Items.Refresh();
            }
        }

        private void BtnRetirerEtape_Click(object sender, RoutedEventArgs e)
        {
            if (lstItineraire.SelectedItem is not PointArret arret)
                return;

            _etapes.Remove(arret);
            lstItineraire.Items.Refresh();
        }

        private void BtnMonterEtape_Click(object sender, RoutedEventArgs e)
        {
            var index = lstItineraire.SelectedIndex;
            if (index <= 0)
                return;

            var item = _etapes[index];
            _etapes.RemoveAt(index);
            _etapes.Insert(index - 1, item);

            lstItineraire.Items.Refresh();
            lstItineraire.SelectedIndex = index - 1;
        }

        private void BtnDescendreEtape_Click(object sender, RoutedEventArgs e)
        {
            var index = lstItineraire.SelectedIndex;
            if (index < 0 || index >= _etapes.Count - 1)
                return;

            var item = _etapes[index];
            _etapes.RemoveAt(index);
            _etapes.Insert(index + 1, item);

            lstItineraire.Items.Refresh();
            lstItineraire.SelectedIndex = index + 1;
        }

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTrain.SelectedItem is not Train train || _etapes.Count < 2)
            {
                ShowError(
                    "Veuillez sélectionner un train et au moins deux arrêts (départ et arrivée)."
                );
                return;
            }

            if (chkRespecteRegles.IsChecked != true)
            {
                ShowError("Les règles de sécurité doivent être respectées.");
                return;
            }

            if (train.Itineraire != null && train.Etat == EtatTrain.EnTransit)
            {
                ShowError($"Le train '{train.Nom}' a déjà un itinéraire en cours.");
                return;
            }

            if (_etapes.First().Id == _etapes.Last().Id)
            {
                ShowError("Le départ et l’arrivée ne peuvent pas être la même station.");
                return;
            }

            if (_etapes.GroupBy(e => e.Id).Any(g => g.Count() > 1))
            {
                ShowError("L’itinéraire contient des doublons d’arrêts.");
                return;
            }

            if (dtDepart.Value == null || dtArrivee.Value == null)
            {
                ShowError("Veuillez choisir une date et une heure de départ et d'arrivée.");
                return;
            }

            var depart = dtDepart.Value.Value;
            var arrivee = dtArrivee.Value.Value;

            if (arrivee <= depart)
            {
                ShowError("La date/heure d'arrivée doit être après la date/heure de départ.");
                return;
            }

            DateDepart = depart;
            DateArrivee = arrivee;
            TrainSelectionne = train;
            ArretsSelectionnes = _etapes.ToList();

            DialogResult = true;
            Close();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
