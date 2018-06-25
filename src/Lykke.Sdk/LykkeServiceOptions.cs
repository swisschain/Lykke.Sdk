using System;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Sdk
{
    [PublicAPI]
    public class LykkeServiceOptions<TAppSettings>
    {
        /// <summary>
        /// Title for Swagger page. Required
        /// </summary>
        public string ApiTitle { get; set; }

        /// <summary>
        /// Logging configuration delegate. Required.
        /// </summary>
        public Action<LykkeLoggingOptions<TAppSettings>> Logs { get; set; }

        /// <summary>
        /// Extended swagger configuration delegate. Optional
        /// </summary>
        [CanBeNull]
        public Action<SwaggerGenOptions> Swagger { get; set; }
    }
}