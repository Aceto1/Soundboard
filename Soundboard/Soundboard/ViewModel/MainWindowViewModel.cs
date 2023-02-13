using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using Soundboard.Model;
using Soundboard.View.EditButtonDialog;
using Soundboard.View.SettingsDialog;
using Soundboard.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Soundboard.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly WaveOutEvent playbackDevice;
        private SerialPort? serialPort;
        private int currentlyPlayingIndex;
        private ObservableCollection<ButtonConfig> buttonConfigs;
        private bool editMode;
        private ApplicationConfig appConfig;

        private static IDialogCoordinator dialogCoordinator;
       
        private string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Soundboard", "config.json");

        public MainWindowViewModel(IDialogCoordinator _dialogCoordinator)
        {
            dialogCoordinator = _dialogCoordinator;

            var dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Soundboard");

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            appConfig = ReadConfig();

            playbackDevice = new WaveOutEvent { DesiredLatency = 200 };
            playbackDevice.Volume = 0.2f;
            playbackDevice.PlaybackStopped += (s, e) =>
            {
                currentlyPlayingIndex = -1;
            };

            currentlyPlayingIndex = -1;

            if (!string.IsNullOrEmpty(appConfig.PlayBackDeviceName))
            {
                var waveInDevices = WaveOut.DeviceCount;
                for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                {
                    var deviceInfo = WaveOut.GetCapabilities(waveInDevice);
                    if (!deviceInfo.ProductName.StartsWith(appConfig.PlayBackDeviceName)) 
                        continue;
                    
                    playbackDevice.DeviceNumber = waveInDevice;
                    break;
                }
            }
            
            ButtonConfigs = new ObservableCollection<ButtonConfig>(appConfig.ButtonConfigs);

            SettingsButtonClickCommand = new RelayCommand(async (obj) =>
            {
                var customDialog = new CustomDialog { Title = "Settings" };

                var dataContext = new SettingsDialogViewModel(settings =>
                {
                    dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                    if (settings != null)
                    {
                        if (settings.LaunchOnStartup)
                            InstallOnStartUp();
                        else
                            UninstallOnStartUp();

                        if (!string.IsNullOrEmpty(settings.PlaybackDeviceName))
                        {
                            var waveInDevices = WaveOut.DeviceCount;
                            for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                            {
                                var deviceInfo = WaveOut.GetCapabilities(waveInDevice);
                                if (!deviceInfo.ProductName.StartsWith(settings.PlaybackDeviceName))
                                    continue;

                                appConfig.PlayBackDeviceName = settings.PlaybackDeviceName;
                                playbackDevice.DeviceNumber = waveInDevice;
                                break;
                            }
                        }

                        appConfig.SerialPortName = settings.SerialPortName;
                        if (!string.IsNullOrEmpty(settings.SerialPortName))
                        {
                            ConnectSerialDevice(settings.SerialPortName);
                        }
                        else
                        {
                            serialPort?.Close();
                            serialPort?.Dispose();
                        }
                    }
                }, appConfig);

                customDialog.Content = new SettingsDialog { DataContext = dataContext };

                await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
            });

            ButtonClickCommand = new RelayCommand(async (obj) =>
            {
                if (editMode || ButtonConfigs.FirstOrDefault(m => m.Index == (int)obj) == null)
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
                }
                else
                {
                    PlaySound((int)obj);
                }
            });
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

        public void ConnectSerialDevice(string serialPortName)
        {
            serialPort?.Close();
            serialPort?.Dispose();

            serialPort = new SerialPort(serialPortName, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            serialPort.Open();
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

        public bool EditMode
        {
            get => editMode;
            set
            {
                if (value == editMode) return;
                editMode = value;
                OnPropertyChanged();
            }
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
                    ButtonConfigs = new List<ButtonConfig>(),
                    PlayBackDeviceName = ""
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
                SerialPortName = appConfig.SerialPortName,
                PlayBackDeviceName = appConfig.PlayBackDeviceName
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

        public RelayCommand SettingsButtonClickCommand { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
