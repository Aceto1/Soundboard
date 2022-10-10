using NAudio.Wave;
using Soundboard.Model;
using Soundboard.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace Soundboard.View.SettingsDialog
{
    internal class SettingsDialogViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> availablePorts;
        private ObservableCollection<string> availablePlaybackDevices;
        private static bool launchOnStartup;

        private static string serialPortName;
        private static string playbackDeviceName;

        private static Action<Settings?> closeHandler;
        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsDialogViewModel(Action<Settings?> _closeHandler, ApplicationConfig currentConfig)
        {
            closeHandler = _closeHandler;
            AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());

            var paybackDevices = new List<string>();

            var waveInDevices = WaveOut.DeviceCount;
            for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                var deviceInfo = WaveOut.GetCapabilities(waveInDevice);

                paybackDevices.Add(deviceInfo.ProductName);
            }

            AvailablePlaybackDevices = new ObservableCollection<string>(paybackDevices);

            SerialPortName = currentConfig.SerialPortName;
            PlaybackDeviceName = currentConfig.PlayBackDeviceName;

            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            var shortcutFileName = startupFolderPath + @"\Soundboard.lnk";

            if (File.Exists(shortcutFileName))
                LaunchOnStartup = true;

            RefreshComPortsCommand = new RelayCommand(obj =>
            {
                AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            });

            RefreshPlaybackDevicesCommand = new RelayCommand(obj =>
            {
                var paybackDevices = new List<string>();

                var waveInDevices = WaveOut.DeviceCount;
                for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                {
                    var deviceInfo = WaveOut.GetCapabilities(waveInDevice);

                    paybackDevices.Add(deviceInfo.ProductName);
                }

                AvailablePlaybackDevices = new ObservableCollection<string>(paybackDevices);
            });
        }

        public string SerialPortName
        {
            get => serialPortName;
            set
            {
                serialPortName = value;
                //ConnectSerialDevice(value);
                OnPropertyChanged(nameof(SerialPortName));
            }
        }

        public bool LaunchOnStartup
        {
            get => launchOnStartup;
            set
            {
                launchOnStartup = value;
                OnPropertyChanged(nameof(LaunchOnStartup));
            }
        }

        public ObservableCollection<string> AvailablePorts
        {
            get => availablePorts;
            set
            {
                availablePorts = value;
                OnPropertyChanged(nameof(AvailablePorts));
            }
        }

        public ObservableCollection<string> AvailablePlaybackDevices
        {
            get => availablePlaybackDevices;
            set
            {
                availablePlaybackDevices = value;
                OnPropertyChanged(nameof(AvailablePlaybackDevices));
            }
        }

        public string PlaybackDeviceName 
        { 
            get => playbackDeviceName; 
            set
            {
                if (playbackDeviceName == value) return;
                playbackDeviceName = value;
                OnPropertyChanged(nameof(PlaybackDeviceName));
            }
        }

        public static void Close(Settings? settings = null)
        {
            closeHandler?.Invoke(settings);
        }

        public RelayCommand SaveCommand = new RelayCommand((obj) =>
        {
            Close(new Settings
            {
                SerialPortName = serialPortName,
                LaunchOnStartup = launchOnStartup,
                PlaybackDeviceName = playbackDeviceName
            });
        });

        public RelayCommand CancelCommand = new RelayCommand(obj => Close());

        public RelayCommand RefreshComPortsCommand;

        public RelayCommand RefreshPlaybackDevicesCommand;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
