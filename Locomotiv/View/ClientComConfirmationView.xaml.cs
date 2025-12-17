using System.Windows;

namespace Locomotiv.View
{
    public partial class ClientComConfirmationView : Window
    {
        public string Message { get; }

        public ClientComConfirmationView(string message)
        {
            InitializeComponent();
            Message = message;
            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}