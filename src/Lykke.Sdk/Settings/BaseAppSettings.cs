using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Sdk.Settings
{
    [PublicAPI]
    public class BaseAppSettings
    {        
        public SlackNotificationsSettings SlackNotifications { get; set; }        
        [Optional]
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
