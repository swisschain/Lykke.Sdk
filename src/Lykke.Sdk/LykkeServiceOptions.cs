using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Sdk
{
    /// <summary>
    /// Options for configuring lykke services.
    /// </summary>
    /// <typeparam name="TAppSettings">The type of the application settings.</typeparam>
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

        /// <summary>
        /// Extended service configuration calls.
        /// </summary>
        [CanBeNull]
        public Action<IServiceCollection, IReloadingManager<TAppSettings>> Extend { get; set; }

        /// <summary>
        ///  Register additional AutoFac modules.
        /// </summary>
        [CanBeNull]
        public Action<IModuleRegistration> RegisterAdditionalModules { get; set; }
        
        /// <summary>
        ///  Extends mvc options. Optional
        /// </summary>
        [CanBeNull]
        public Action<MvcOptions> ConfigureMvcOptions { get; set; }
    }
}
