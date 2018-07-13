using JetBrains.Annotations;

namespace Lykke.Sdk.Settings
{
    /// <summary>
    /// The slack notification settings.
    /// </summary>
    [PublicAPI]
    public class SlackNotificationsSettings
    {
        /// <summary>
        /// The azure queue settings.
        /// </summary>
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }
}
