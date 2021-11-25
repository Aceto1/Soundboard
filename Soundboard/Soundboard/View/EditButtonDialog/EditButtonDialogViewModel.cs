using MahApps.Metro.Controls.Dialogs;
using Soundboard.Model;
using Soundboard.ViewModel.Base;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Soundboard.View.EditButtonDialog
{
    internal class EditButtonDialogViewModel: INotifyPropertyChanged
    {
        private string filepath;
        private static Action<ButtonConfig> closeHandler;
        public event PropertyChangedEventHandler? PropertyChanged;

        public EditButtonDialogViewModel(Action<ButtonConfig> _closeHandler)
        {
            closeHandler = _closeHandler;
        }

        public string Filepath
        {
            get => filepath;
            set
            {
                filepath = value;
                OnPropertyChanged(nameof(Filepath));
            }
        }

        public static void Close()
        {
            closeHandler?.Invoke(null);
        }

        public RelayCommand ChooseFileCommand = new RelayCommand((obj) =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                var fullPath = dialog.FileName;
            }            
        });

        public RelayCommand SaveCommand = new RelayCommand((obj) =>
        {
            Close();
        });

        public RelayCommand CancelCommand = new RelayCommand(obj => Close());

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
