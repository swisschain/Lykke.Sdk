using System;
using JetBrains.Annotations;
using Lykke.Sdk.Health;
using Microsoft.AspNetCore.Builder;

namespace Lykke.Sdk.Middleware
{
    /// <summary>
    /// Maintenance middleware builder.
    /// </summary>
    [PublicAPI]
    public static class MaintenanceModeBuilder
    {
        /// <summary>
        /// Adds middleware that checks if the service is in maintenance mode.
        /// </summary>
        /// <typeparam name="TAppSettings">The type of the application settings.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="getMode">The maintenance mode getter.</param>
        /// <returns>the builder</returns>
        public static IApplicationBuilder UseMaintenanceMode<TAppSettings>(this IApplicationBuilder builder, Func<TAppSettings, MaintenanceMode> getMode)
        {
            if (getMode == null)
                throw new ArgumentNullException(nameof(getMode));

            return builder.UseMiddleware<MaintenanceModeMiddleware<TAppSettings>>(getMode);
        }
    }
}