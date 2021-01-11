using JetBrains.Annotations;

namespace Antares.Sdk.Settings
{
    /// <summary>
    /// General app settings abstraction
    /// </summary>
    [PublicAPI]
    public interface IAppSettings
    {
        /// <summary>
        /// The slack notification settings.
        /// </summary>
        SlackNotificationsSettings SlackNotifications { get; }

        /// <summary>
        /// The monitoring service settings.
        /// </summary>
        MonitoringServiceClientSettings MonitoringServiceClient { get; }
    }
}