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
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeApplicationBuilderExtensions
    {
        public static void UseLykkeConfiguration(this IApplicationBuilder app, Action<LykkeAppOptions> optionBuilder)
        {
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var log = app.ApplicationServices.GetService<ILog>();

            try
            {
                var options = new LykkeAppOptions();
                optionBuilder(options);
                
                var appLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
                var configurationRoot = app.ApplicationServices.GetService<IConfigurationRoot>();

                if (configurationRoot == null)
                    throw new ApplicationException("Configuration root must be registered in the container");

                var monitoringSettings = app.ApplicationServices.GetService<IReloadingManager<MonitoringServiceClientSettings>>();
                
                var startupManager = app.ApplicationServices.GetService<IStartupManager>();
                var shutdownManager = app.ApplicationServices.GetService<IShutdownManager>();
                var serviceOptions = app.ApplicationServices.GetService<LykkeServiceOptions>();

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

                var appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                
                app.UseLykkeForwardedHeaders();
                app.UseLykkeMiddleware(appName, ex => new
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
                    x.SwaggerEndpoint($"/swagger/{serviceOptions.ApiVersion}/swagger.json", serviceOptions.ApiVersion);
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
