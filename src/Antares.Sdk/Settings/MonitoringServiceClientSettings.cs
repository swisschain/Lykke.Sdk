using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Antares.Sdk.Settings
{
    /// <summary>
    /// Monitoring settings class
    /// </summary>
    [PublicAPI]
    public class MonitoringServiceClientSettings
    {
        /// <summary>
        /// Gets or sets the monitoring service URL.
        /// </summary>
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
