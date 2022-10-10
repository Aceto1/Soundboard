using System.Collections.Generic;

namespace Soundboard.Model
{
    public class ApplicationConfig
    {
        public string SerialPortName { get; set; }

        public string PlayBackDeviceName { get; set; }

        public List<ButtonConfig> ButtonConfigs { get; set; }
    }
}
