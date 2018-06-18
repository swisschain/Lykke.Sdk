using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;

namespace Lykke.Sdk
{
    /// <summary>
    /// Class for configuration overrides in UseLykkeConfiguration
    /// </summary>
    [PublicAPI]
    public class LykkeConfigurationOptions
    {
        /// <summary>Default error handler.</summary>
        public CreateErrorResponse DefaultErrorHandler { get; set; }
    }
}
