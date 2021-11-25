using MahApps.Metro.Controls.Dialogs;
using Soundboard.Model;
using System;
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

        }

        private void TitleChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SelectFileButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void CancelClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as EditButtonDialogViewModel)?.CancelCommand.Execute(e);
        }
    }
}
