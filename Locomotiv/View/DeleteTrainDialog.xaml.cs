using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Locomotiv.Model;

namespace Locomotiv.View
{
    /// <summary>
    /// Logique d'interaction pour DeleteTrainDialog.xaml
    /// </summary>
    public partial class DeleteTrainDialog : Window
    {
        private readonly List<Station> _stations;
        public Train? TrainASupprimer { get; private set; }

        public DeleteTrainDialog(List<Station> stations)
        {
            InitializeComponent();
            _stations = stations;
            cmbStation.ItemsSource = _stations;
        }

        private void cmbStation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var station = cmbStation.SelectedItem as Station;
            if (station != null)
            {
                cmbTrain.ItemsSource = station.Train;
            }
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            TrainASupprimer = cmbTrain.SelectedItem as Train;
            DialogResult = TrainASupprimer != null;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
