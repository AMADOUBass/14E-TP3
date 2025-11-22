using System.Windows;
using Locomotiv.Model;
using Locomotiv.Model.enums;

namespace Locomotiv.View
{
    /// <summary>
    /// Fenêtre d’ajout d’un train :
    /// - saisie du nom
    /// - état
    /// - capacité
    /// - station
    /// </summary>
    public partial class TrainFormDialog : Window
    {
        public Train Train { get; private set; } = null!;

        // 🔎 Nouveau constructeur qui prend les stations
        public TrainFormDialog(List<Station> stations)
        {
            InitializeComponent();

            cmbStation.ItemsSource = stations;
        }

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show(
                    "Veuillez entrer un nom pour le train.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (cmbEtat.SelectedItem is not EtatTrain etat)
            {
                MessageBox.Show(
                    "Veuillez sélectionner un état pour le train.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!int.TryParse(txtCapacite.Text, out int capacite) || capacite <= 0)
            {
                MessageBox.Show(
                    "Veuillez entrer une capacité valide (nombre positif).",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

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

            var trainsDansStation = station.Train?.Count ?? 0;
            if (trainsDansStation >= station.CapaciteMaxTrains)
            {
                MessageBox.Show(
                    $"La station '{station.Nom}' est déjà à capacité maximale.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            Train = new Train
            {
                Nom = txtNom.Text.Trim(),
                Etat = etat,
                Capacite = capacite,
                StationId = (int)cmbStation.SelectedValue,
                Station = station,
                Itineraire = null,
            };

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
