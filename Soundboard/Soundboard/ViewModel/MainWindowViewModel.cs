using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using Soundboard.Model;
using Soundboard.View.EditButtonDialog;
using Soundboard.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Soundboard.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private WaveOutEvent playbackDevice;
        private string serialPortName;
        private SerialPort serialPort;
        private int currentlyPlayingIndex;
        private ObservableCollection<string> availablePorts;
        private ObservableCollection<ButtonConfig> buttonConfigs;
        private static IDialogCoordinator dialogCoordinator;
        private bool launchOnStartup;
        private string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Soundboard", "config.json");

        public MainWindowViewModel(IDialogCoordinator _dialogCoordinator)
        {
            dialogCoordinator = _dialogCoordinator;

            playbackDevice = new WaveOutEvent { DesiredLatency = 200 };
            playbackDevice.Volume = 0.2f;
            playbackDevice.PlaybackStopped += (s, e) =>
            {
                currentlyPlayingIndex = -1;
            };

            currentlyPlayingIndex = -1;

            int waveInDevices = WaveOut.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveInDevice);
                if (deviceInfo.ProductName.StartsWith("VoiceMeeter Aux Input"))
                {
                    playbackDevice.DeviceNumber = waveInDevice;
                    break;
                }
            }

            AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());

            var dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Soundboard");

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var appConfig = ReadConfig();

            ButtonConfigs = new ObservableCollection<ButtonConfig>(appConfig.ButtonConfigs);
            SerialPortName = appConfig.SerialPortName;

            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            var shortcutFileName = startupFolderPath + @"\Soundboard.lnk";

            if (File.Exists(shortcutFileName))
                LaunchOnStartup = true;

            ButtonClickCommand = new RelayCommand(async (obj) =>
            {
                var customDialog = new CustomDialog { Title = "Edit Button" };

                var existingConfig = ButtonConfigs.SingleOrDefault(m => m.Index == (int)obj);

                var dataContext = new EditButtonDialogViewModel(buttonConfig =>
                {
                    dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                    if (buttonConfig != null)
                    {
                        buttonConfig.Index = (int)obj;
                        EditButtonConfig(buttonConfig);
                    }
                })
                {
                    Title = existingConfig?.Title ?? "",
                    Filepath = existingConfig?.FilePath ?? ""
                };

                customDialog.Content = new EditButtonDialog { DataContext = dataContext };

                await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
            });
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

        public ObservableCollection<ButtonConfig> ButtonConfigs
        {
            get => buttonConfigs;
            set
            {
                buttonConfigs = value;
                OnPropertyChanged(nameof(ButtonConfigs));
            }
        }

        public string SerialPortName
        {
            get => serialPortName;
            set
            {
                serialPortName = value;
                ConnectSerialDevice(value);
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
                if (value)
                    InstallOnStartUp();
                else
                    UninstallOnStartUp();
            }
        }

        void InstallOnStartUp()
        {
            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            var shortcutFileName = startupFolderPath + @"\Soundboard.lnk";

            if (File.Exists(shortcutFileName))
                return;

            // COM object instance/props
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutFileName);

            //shortcut.IconLocation = @"C:\..."; 
            sc.TargetPath = AppContext.BaseDirectory + @"\Soundboard.exe";
            sc.WorkingDirectory = AppContext.BaseDirectory;
            // save shortcut to target
            sc.Save();
        }

        void UninstallOnStartUp()
        {
            var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            var shortcutFileName = startupFolderPath + @"\Soundboard.lnk";

            if (!File.Exists(shortcutFileName))
                return;

            File.Delete(shortcutFileName);
        }

        public void ConnectSerialDevice(string serialPortName)
        {
            if (!AvailablePorts.Contains(serialPortName))
                return;

            if (serialPort != null)
            {
                serialPort.Dispose();
            }

            serialPort = new SerialPort(serialPortName, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            serialPort.Open();
        }

        private ApplicationConfig ReadConfig()
        {
            string jsonString;
            try
            {
                jsonString = File.ReadAllText(configPath);
            }
            catch (Exception)
            {
                return new ApplicationConfig
                {
                    SerialPortName = "",
                    ButtonConfigs = new List<ButtonConfig>()
                };
            }

            ApplicationConfig appConfig = JsonSerializer.Deserialize<ApplicationConfig>(jsonString)!;

            return appConfig;
        }

        public void SaveConfig()
        {
            var config = new ApplicationConfig()
            {
                ButtonConfigs = ButtonConfigs.ToList(),
                SerialPortName = serialPortName
            };

            string jsonString = JsonSerializer.Serialize(config);

            File.WriteAllText(configPath, jsonString);
        }

        private void PlaySound(int index)
        {
            if(playbackDevice.PlaybackState == PlaybackState.Playing)
            {
                playbackDevice.Stop();

                // Pressed the same button as the one currently playing => just stop playback
                // Otherwise play sound of button pressed after stopping
                if (currentlyPlayingIndex == index)
                    return;
            }

            var buttonConfig = ButtonConfigs.SingleOrDefault(m => m.Index == index);

            if (buttonConfig == null)
                return;

            var reader = new AudioFileReader(buttonConfig.FilePath);
            playbackDevice.Init(reader);
            playbackDevice.Play();
            currentlyPlayingIndex = index;
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();

            if (int.TryParse(data, out int index))
                PlaySound(index);
        }

        private void EditButtonConfig(ButtonConfig config)
        {
            var existingButtonConfig = ButtonConfigs.SingleOrDefault(m => m.Index == config.Index);

            if (existingButtonConfig == null)
            {
                ButtonConfigs.Add(config);
                ButtonConfigs = new ObservableCollection<ButtonConfig>(ButtonConfigs);
            }
            else
            {
                ButtonConfigs.Remove(existingButtonConfig);
                ButtonConfigs.Add(config);
                ButtonConfigs = new ObservableCollection<ButtonConfig>(ButtonConfigs);
            }
        }

        public RelayCommand ButtonClickCommand { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
