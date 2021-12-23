using System.Windows.Controls;

namespace Soundboard.View.EditButtonDialog
{
    /// <summary>
    /// Interaction logic for EditButtonDialog.xaml
    /// </summary>
    public partial class EditButtonDialog : UserControl
    {
        public EditButtonDialog()
        {
            InitializeComponent();
        }

        private void SaveClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as EditButtonDialogViewModel)?.SaveCommand.Execute(e);
        }

        private void TitleChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as EditButtonDialogViewModel).Title = Title.Text;
        }

        private void SelectFileButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                Filepath.Text = dialog.FileName;
                (this.DataContext as EditButtonDialogViewModel).Filepath = dialog.FileName;
            }
        }

        private void CancelClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as EditButtonDialogViewModel)?.CancelCommand.Execute(e);
        }
    }
}
