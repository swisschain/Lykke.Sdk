using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Sdk.Settings
{
    [PublicAPI]
    public class MonitoringServiceClientSettings
    {        
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }
}
