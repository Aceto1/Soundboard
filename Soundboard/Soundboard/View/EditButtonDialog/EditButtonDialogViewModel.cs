using Soundboard.Model;
using Soundboard.ViewModel.Base;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Soundboard.View.EditButtonDialog
{
    internal class EditButtonDialogViewModel : INotifyPropertyChanged
    {
        private static string filepath;
        private static string title;
        private static Action<ButtonConfig?> closeHandler;
        public event PropertyChangedEventHandler? PropertyChanged;

        public EditButtonDialogViewModel(Action<ButtonConfig?> _closeHandler)
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

        public string Title 
        { 
            get => title; 
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            } 
        }

        public static void Close(ButtonConfig? buttonConfig = null)
        {
            closeHandler?.Invoke(buttonConfig);
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
            if (String.IsNullOrWhiteSpace(filepath) || String.IsNullOrWhiteSpace(title))
                return;

            Close(new ButtonConfig
            {
                FilePath = filepath,
                Title = title
            });
        });

        public RelayCommand CancelCommand = new RelayCommand(obj => Close());

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
