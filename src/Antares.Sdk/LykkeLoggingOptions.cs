using System;
using JetBrains.Annotations;
using Lykke.Logs;

namespace Antares.Sdk
{
    /// <summary>
    /// Lykke logging options class.
    /// </summary>
    /// <typeparam name="TAppSettings">The type of the application settings.</typeparam>
    [PublicAPI]
    public class LykkeLoggingOptions<TAppSettings>
    {
        /// <summary>
        /// Name of the Azure table for logs. Required
        /// </summary>
        public string AzureTableName { get; set; }

        /// <summary>
        /// Azure table connection string resolver delegate for Azure table logs. Required
        /// </summary>
        public Func<TAppSettings, string> AzureTableConnectionStringResolver { get; set; }

        /// <summary>
        /// Extended logging options. Optional
        /// </summary>
        [CanBeNull]
        public Action<ILogBuilder> Extended { get; set; }

        /// <summary>
        /// This flag indicates whether empty logging system should be used
        /// </summary>
        public bool HaveToUseEmptyLogging { get; private set; }

        /// <summary>
        /// Setup logging system to log nothing. Another options could be not specified in this case.
        /// </summary>
        public void UseEmptyLogging()
        {
            HaveToUseEmptyLogging = true;
        }
    }
}