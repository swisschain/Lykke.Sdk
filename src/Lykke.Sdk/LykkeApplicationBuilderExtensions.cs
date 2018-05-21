using System;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeApplicationBuilderExtensions
    {
        public static void UseLykkeConfiguration(this IApplicationBuilder app, Action<LykkeAppOptions> optionBuilder)
        {
            var log = (ILog)app.ApplicationServices.GetService(typeof(ILog));

            try
            {
                var options = new LykkeAppOptions();
                optionBuilder(options);

                if (string.IsNullOrWhiteSpace(options.AppName))
                    throw new ApplicationException("Application name is required.");

                if (string.IsNullOrWhiteSpace(options.Version))
                    throw new ApplicationException("Version is required.");

                var appLifetime = (IApplicationLifetime) app.ApplicationServices.GetService(typeof(IApplicationLifetime));
                var configurationRoot = (IConfigurationRoot) app.ApplicationServices.GetService(typeof(IConfigurationRoot));
                var monitoringSettings = (IReloadingManager<MonitoringServiceClientSettings>) app.ApplicationServices.GetService(typeof(IReloadingManager<MonitoringServiceClientSettings>));
                
                var startupManager = (IStartupManager) app.ApplicationServices.GetService(typeof(IStartupManager));
                var shutdownManager = (IShutdownManager) app.ApplicationServices.GetService(typeof(IShutdownManager));

                appLifetime.ApplicationStarted.Register(() =>
                {
                    try
                    {
                        startupManager?.StartAsync().GetAwaiter().GetResult();

                        log.WriteMonitor("StartApplication", null, "Application started");

                        if (!options.IsDebug)
                        {
                            if (monitoringSettings.CurrentValue == null)
                                throw new ApplicationException("Monitoring settings is not provided.");

                            AutoRegistrationInMonitoring.RegisterAsync(configurationRoot, monitoringSettings.CurrentValue.MonitoringServiceUrl, log).GetAwaiter().GetResult();
                        }

                    }
                    catch (Exception ex)
                    {
                        log.WriteFatalError("StartApplication", "", ex);
                        throw;
                    }
                });

                appLifetime.ApplicationStopping.Register(() =>
                {
                    try
                    {
                        shutdownManager?.StopAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        log?.WriteFatalError("StopApplication", "", ex);

                        throw;
                    }
                });

                appLifetime.ApplicationStopped.Register(() =>
                {
                    try
                    {
                        log?.WriteMonitor("StopApplication", null, "Terminating");
                    }
                    catch (Exception ex)
                    {
                        if (log != null)
                        {
                            log.WriteFatalError("CleanUp", "", ex);
                            (log as IDisposable)?.Dispose();
                        }
                        throw;
                    }
                });


                app.UseLykkeForwardedHeaders();
                app.UseLykkeMiddleware(options.AppName, ex => new
                {
                    Message = "Technical problem"
                });

                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint($"/swagger/{options.Version}/swagger.json", options.Version);
                });
                app.UseStaticFiles();
            }
            catch (Exception ex)
            {
                log?.WriteFatalError("Startup", "", ex);
                throw;
            }
        }
    }    
}
