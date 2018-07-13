using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Sdk.Health;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Http;

namespace Lykke.Sdk.Middleware
{
    internal sealed class MaintenanceModeMiddleware<TAppSettings> : TimerPeriod
    {
        private readonly IReloadingManager<TAppSettings> _appSettings;
        private readonly RequestDelegate _next;
        private readonly Func<TAppSettings, MaintenanceMode> _getMode;

        [UsedImplicitly]
        public MaintenanceModeMiddleware(RequestDelegate next, IReloadingManager<TAppSettings> appSettings, Func<TAppSettings, MaintenanceMode> getMode, ILogFactory logFactory, TimeSpan period)
            : base(period, logFactory)
        {
            _next = next;
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _getMode = getMode ?? throw new ArgumentNullException(nameof(getMode));

            Start();
        }

        public MaintenanceMode Mode { get; set; }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            if (Mode != null && Mode.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

                var reason = !string.IsNullOrWhiteSpace(Mode.Reason) ? Mode.Reason : "Service on maintenance";
                await context.Response.WriteAsync(reason);
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        public override async Task Execute()
        {
            await _appSettings.Reload();
            Mode = _getMode(_appSettings.CurrentValue);
        }
    }
}