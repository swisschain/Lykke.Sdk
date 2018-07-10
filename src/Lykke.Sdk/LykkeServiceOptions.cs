using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Sdk
{
    [PublicAPI]
    public class LykkeServiceOptions<TAppSettings>
    {
        /// <summary>
        /// Swagger Options. Required
        /// </summary>
        [Required]
        public LykkeSwaggerOptions SwaggerOptions { get; set; }

        /// <summary>
        /// Logging configuration delegate. Required.
        /// </summary>
        [Required]
        public Action<LykkeLoggingOptions<TAppSettings>> Logs { get; set; }

        /// <summary>
        /// Extended swagger configuration delegate. Optional
        /// </summary>
        [CanBeNull]
        public Action<SwaggerGenOptions> Swagger { get; set; }
    }
}