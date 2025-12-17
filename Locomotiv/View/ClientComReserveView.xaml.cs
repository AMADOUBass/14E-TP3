using Locomotiv.ViewModel;
using System.Windows;

namespace Locomotiv.View
{
    public partial class ClientComReserveView : Window
    {
        public ClientComReserveView(ClientComReserveViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.RequestClose += () => this.Close();

            vm.RequestConfirmation += (message, title) =>
            {
                var confirmationWindow = new ClientComConfirmationView(message)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                confirmationWindow.ShowDialog();
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}