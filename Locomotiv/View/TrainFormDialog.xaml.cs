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
    /// Logique d'interaction pour TrainFormDialog.xaml
    /// </summary>
    public partial class TrainFormDialog : Window

    {
        public Train Train { get; private set; }
        public List<Station> Stations { get; set; }
        public TrainFormDialog(List<Station> stations)
        {
            InitializeComponent();
            Stations = stations;
            cmbStation.ItemsSource = Stations;
        }
        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            Train = new Train
            {

                Nom = txtNom.Text,
                Etat = (EtatTrain)cmbEtat.SelectedItem,
                Capacite = int.Parse(txtCapacite.Text),
                StationId = (int)cmbStation.SelectedValue,
                Station = (Station)cmbStation.SelectedItem,
                Itineraire = null, // L'itinéraire peut être défini ultérieurement
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
