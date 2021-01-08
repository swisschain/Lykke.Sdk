using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Antares.Sdk.Settings
{
    /// <summary>
    /// Base class for lykke settings
    /// </summary>
    [PublicAPI]
    public class BaseAppSettings : IAppSettings
    {
        /// <inheritdoc />
        [Optional]
        public SlackNotificationsSettings SlackNotifications { get; set; }

        /// <inheritdoc />
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
