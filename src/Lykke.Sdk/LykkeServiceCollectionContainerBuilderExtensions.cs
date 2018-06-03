using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeServiceCollectionContainerBuilderExtensions
    {
        /// <summary>
        /// Build service provider for Lykke's service.
        /// </summary>        
        public static IServiceProvider BuildServiceProvider<TAppSettings>(this IServiceCollection services, Action<LykkeServiceOptions<TAppSettings>> serviceOptionsBuilder)
            where TAppSettings : BaseAppSettings
        {
            if (services == null)
                throw new ArgumentNullException("services");

            if (serviceOptionsBuilder == null)
                throw new ArgumentNullException("serviceOptionsBuilder");

            var serviceOptions = new LykkeServiceOptions<TAppSettings>();
            serviceOptionsBuilder(serviceOptions);
            
            if (string.IsNullOrWhiteSpace(serviceOptions.ApiTitle))
                throw new ArgumentException("Api title must be provided.");

            if (serviceOptions.LogsConnectionStringFactory == null)
                throw new ArgumentException("Logs connection string factory must be provided.");

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", serviceOptions.ApiTitle);
            });

            var configurationRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var settings = configurationRoot.LoadSettings<TAppSettings>();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(configurationRoot).As<IConfigurationRoot>();            
            builder.RegisterInstance(settings.CurrentValue.SlackNotifications);

            if (settings.CurrentValue.MonitoringServiceClient != null)
                builder.RegisterInstance(settings.Nested(x => x.MonitoringServiceClient));            

            builder.RegisterInstance(serviceOptions);

            var logger = LoggerFactory.CreateLogWithSlack(builder, serviceOptions.LogsTableName, serviceOptions.LogsConnectionStringFactory(settings), settings.CurrentValue.SlackNotifications);

            builder.RegisterInstance(logger);
            builder.RegisterAssemblyModules(settings, logger, Assembly.GetCallingAssembly());
            builder.Populate(services);

            var container = builder.Build();

            var appLifetime = container.Resolve<IApplicationLifetime>();
            
            appLifetime.ApplicationStopped.Register(() =>
            {
                try
                {
                    logger?.WriteMonitor("StopApplication", null, "Terminating");

                    container.Dispose();
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.WriteFatalError("CleanUp", "", ex);
                        (logger as IDisposable)?.Dispose();
                    }
                    throw;
                }
            });

            return new AutofacServiceProvider(container);
        }
    }
}
