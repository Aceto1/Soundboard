using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NAudio.Wave;
using Soundboard.Model;
using Soundboard.View.EditButtonDialog;
using Soundboard.ViewModel.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace Soundboard.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private WaveOutEvent playbackDevice;
        private SerialPort port;
        private ObservableCollection<string> availablePorts;
        private ObservableCollection<ButtonConfig> buttonConfigs;
        private static IDialogCoordinator dialogCoordinator;

        public MainWindowViewModel(IDialogCoordinator _dialogCoordinator)
        {
            dialogCoordinator = _dialogCoordinator;

            playbackDevice = new WaveOutEvent { DesiredLatency = 200 };
            playbackDevice.Volume = 0.2f;

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

            //port = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
            //port.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            //port.Open();

            ButtonClickCommand = new RelayCommand(async (obj) =>
            {
                var customDialog = new CustomDialog { Title = "Edit Button" };

                var dataContext = new EditButtonDialogViewModel(buttonConfig =>
                {
                    dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                    EditButtonConfig((int)obj, buttonConfig);
                });

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

        private ButtonConfig[] ReadButtonConfigs()
        {
            return new ButtonConfig[0];
        }

        private void SaveButtonConfigs()
        {

        }

        private void PlaySound(int id)
        {
            var reader = new AudioFileReader("E:\\Musik\\Sounds\\test.mp3");
            playbackDevice.Init(reader);
            playbackDevice.Play();
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
        {
            PlaySound(1);
        }

        private void EditButtonConfig(int id, ButtonConfig config)
        {

        }

        public RelayCommand ButtonClickCommand { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
