using System.Windows.Controls;

namespace Soundboard.View.SettingsDialog
{
    /// <summary>
    /// Interaction logic for EditButtonDialog.xaml
    /// </summary>
    public partial class SettingsDialog : UserControl
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void SaveClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as SettingsDialogViewModel)?.SaveCommand.Execute(e);
        }

        private void CancelClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as SettingsDialogViewModel)?.CancelCommand.Execute(e);
        }

        private void RefreshComPortsClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as SettingsDialogViewModel)?.RefreshComPortsCommand.Execute(e);
        }

        private void RefreshPaybackDevicesClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as SettingsDialogViewModel)?.RefreshPlaybackDevicesCommand.Execute(e);
        }
    }
}
