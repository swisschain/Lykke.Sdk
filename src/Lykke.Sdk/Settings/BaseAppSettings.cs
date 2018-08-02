using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Sdk.Settings
{
    /// <summary>
    /// Base class for lykke settings
    /// </summary>
    [PublicAPI]
    public class BaseAppSettings
    {
        /// <summary>
        /// The slack notification settings.
        /// </summary>
        [Optional]
        public SlackNotificationsSettings SlackNotifications { get; set; }

        /// <summary>
        /// The monitoring service settings.
        /// </summary>
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
