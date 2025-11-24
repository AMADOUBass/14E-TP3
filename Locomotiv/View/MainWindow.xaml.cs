using System.Reflection;
using System.Windows;
using Locomotiv.ViewModel;

namespace Locomotiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            VersionText.Text = $"Version: {version}";
        }
    }
}
