using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Locomotiv.Model;

namespace Locomotiv.View
{
    /// <summary>
    /// Fenêtre de planification d’un itinéraire :
    /// - choix d’un train
    /// - choix et ordre des arrêts (stations / points d’intérêt)
    /// </summary>
    public partial class PlanifierItineraireDialog : Window
    {
        public Train TrainSelectionne { get; private set; } = null!;
        public List<PointArret> ArretsSelectionnes { get; private set; } = new();

        private readonly List<PointArret> _etapes = new();

        public PlanifierItineraireDialog(List<Train> Train, List<PointArret> pointsArret)
        {
            InitializeComponent();

            var trainsDispo = Train.Where(t => t.Etat != EtatTrain.EnTransit).ToList();

            cmbTrain.ItemsSource = trainsDispo;
            lstPointsDisponibles.ItemsSource = pointsArret;
            lstItineraire.ItemsSource = _etapes;

            if (!trainsDispo.Any())
            {
                MessageBox.Show(
                    "Aucun train disponible pour planifier un itinéraire. (tous sont en transit).",
                    "Planification d'itinéraire impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Close();
            }
        }

        private void BtnAjouterEtape_Click(object sender, RoutedEventArgs e)
        {
            if (lstPointsDisponibles.SelectedItem is not PointArret arret)
                return;

            // On évite les doublons si tu veux, sinon enlève ce if
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
            if (index <= 0) return;

            var item = _etapes[index];
            _etapes.RemoveAt(index);
            _etapes.Insert(index - 1, item);

            lstItineraire.Items.Refresh();
            lstItineraire.SelectedIndex = index - 1;
        }

        private void BtnDescendreEtape_Click(object sender, RoutedEventArgs e)
        {
            var index = lstItineraire.SelectedIndex;
            if (index < 0 || index >= _etapes.Count - 1) return;

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
                MessageBox.Show(
                    "Veuillez sélectionner un train et au moins deux arrêts (départ et arrivée).",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (chkRespecteRegles.IsChecked != true)
            {
                MessageBox.Show(
                    "Les règles de sécurité doivent être respectées.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            TrainSelectionne = train;
            ArretsSelectionnes = _etapes.ToList();

            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
