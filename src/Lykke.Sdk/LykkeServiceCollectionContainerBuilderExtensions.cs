using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeServiceCollectionContainerBuilderExtensions
    {
        /// <summary>
        /// Build service provider for Lykke's service.
        /// </summary>        
        public static IServiceProvider BuildServiceProvider<TAppSettings>(this IServiceCollection services, Action<LykkeServiceOptions> serviceOptionsBuilder)
            where TAppSettings : BaseAppSettings
        {
            if (services == null)
                throw new ArgumentNullException("services");

            var serviceOptions = new LykkeServiceOptions();
            serviceOptionsBuilder(serviceOptions);

            if (string.IsNullOrWhiteSpace(serviceOptions.ApiVersion))
                throw new ArgumentException("Api version must be provided.");

            if (string.IsNullOrWhiteSpace(serviceOptions.ApiTitle))
                throw new ArgumentException("Api title must be provided.");

            if (serviceOptions.LogsConnectionStringFactory == null)
                throw new ArgumentException("Logs connection string factory must be provided.");

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration(serviceOptions.ApiVersion, serviceOptions.ApiTitle);
            });

            var configurationRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var settings = configurationRoot.LoadSettings<TAppSettings>();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(configurationRoot).As<IConfigurationRoot>();
            builder.RegisterInstance(settings);
            builder.RegisterInstance(JObject.FromObject(settings.CurrentValue));
            builder.RegisterInstance(serviceOptions);
            builder.RegisterModule(new SdkModule(serviceOptions.LogsConnectionStringFactory, serviceOptions.LogsTableName));
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
