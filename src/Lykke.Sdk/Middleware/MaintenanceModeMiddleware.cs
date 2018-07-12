using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Sdk.Health;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Http;

namespace Lykke.Sdk.Middleware
{
    internal sealed class MaintenanceModeMiddleware<TAppSettings>
    {
        private readonly IReloadingManager<TAppSettings> _appSettings;
        private readonly RequestDelegate _next;
        private readonly Func<TAppSettings, MaintenanceMode> _getMode;

        [UsedImplicitly]
        public MaintenanceModeMiddleware(RequestDelegate next, IReloadingManager<TAppSettings> appSettings, Func<TAppSettings, MaintenanceMode> getMode)
        {
            _next = next;
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _getMode = getMode ?? throw new ArgumentNullException(nameof(getMode));
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            var mode = _getMode(_appSettings.CurrentValue);

            if (mode != null && mode.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

                var reason = !string.IsNullOrWhiteSpace(mode.Reason) ? mode.Reason : "Service on maintenance";
                await context.Response.WriteAsync(reason);
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}