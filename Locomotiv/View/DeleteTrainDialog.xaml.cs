using System.Windows;
using System.Windows.Controls;
using Locomotiv.Model;
using Locomotiv.Model.enums;

namespace Locomotiv.View
{
    /// <summary>
    /// Fenêtre de suppression d’un train :
    /// - choix d’une station
    /// - choix d’un train dans cette station
    /// </summary>
    public partial class DeleteTrainDialog : Window
    {
        private readonly List<Station> _stations;
        public Train? TrainASupprimer { get; private set; }

        public DeleteTrainDialog(List<Station> stations)
        {
            InitializeComponent();
            _stations = stations ?? new List<Station>();
            cmbStation.ItemsSource = _stations;

            if (!_stations.Any())
            {
                MessageBox.Show(
                    "Aucune station disponible. Impossible de supprimer un train.",
                    "Suppression impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Close();
            }
        }

        private void cmbStation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStation.SelectedItem is Station station)
            {
                if (station.Train == null || !station.Train.Any())
                {
                    MessageBox.Show(
                        $"La station '{station.Nom}' ne contient aucun train.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    cmbTrain.ItemsSource = null;
                    return;
                }

                cmbTrain.ItemsSource = station.Train;
            }
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStation.SelectedItem is not Station station)
            {
                MessageBox.Show(
                    "Veuillez sélectionner une station.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (cmbTrain.SelectedItem is not Train train)
            {
                MessageBox.Show(
                    "Veuillez sélectionner un train à supprimer.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (train.Etat == EtatTrain.EnTransit)
            {
                MessageBox.Show(
                    $"Le train '{train.Nom}' est en transit et ne peut pas être supprimé.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (train.Itineraire != null)
            {
                MessageBox.Show(
                    $"Le train '{train.Nom}' possède un itinéraire actif. Supprimez l’itinéraire avant de supprimer le train.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            TrainASupprimer = train;
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
