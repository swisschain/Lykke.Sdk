using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Antares.Sdk.Settings
{
    /// <summary>
    /// Azure queue publication settings
    /// </summary>
    [PublicAPI]
    public class AzureQueuePublicationSettings
    {
        /// <summary>
        /// The azure connection string.
        /// </summary>
        [AzureQueueCheck]
        public string ConnectionString { get; set; }

        /// <summary>
        /// The azure queue name.
        /// </summary>
        public string QueueName { get; set; }
    }
}
