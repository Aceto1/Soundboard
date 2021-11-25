using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Soundboard.ViewModel;

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(DialogCoordinator.Instance);
        }
    }
}
