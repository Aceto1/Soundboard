using System.Collections.Generic;

namespace Soundboard.Model
{
    public class ApplicationConfig
    {
        public string SerialPortName { get; set; }

        public List<ButtonConfig> ButtonConfigs { get; set; }
    }
}
