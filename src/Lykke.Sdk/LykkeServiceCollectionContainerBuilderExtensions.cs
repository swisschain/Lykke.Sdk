using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeServiceCollectionContainerBuilderExtensions
    {
        public static IServiceProvider BuildServiceProvider<TAppSettings>(this IServiceCollection services,
            Func<IReloadingManager<TAppSettings>, IReloadingManager<string>> logsConnectionStringFactory)
            where TAppSettings : BaseAppSettings
        {
            var configurationRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var settings = configurationRoot.LoadSettings<TAppSettings>();

            var builder = new ContainerBuilder();
            
            builder.RegisterModule(new SdkModule<TAppSettings>(settings, logsConnectionStringFactory));
            builder.RegisterAssemblyModules(Assembly.GetCallingAssembly());
            builder.Populate(services);

            var container = builder.Build();

            var appLifetime = container.Resolve<IApplicationLifetime>();
            var log = container.Resolve<ILog>();
            
            appLifetime.ApplicationStopped.Register(() =>
            {
                try
                {
                    log?.WriteMonitor("StopApplication", null, "Terminating");

                    container.Dispose();
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
            
            return new AutofacServiceProvider(container);
        }
    }
}
