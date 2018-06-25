using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System;
using System.Reflection;
using Lykke.Logs;
using Lykke.Sdk.Settings;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeServiceCollectionContainerBuilderExtensions
    {
        /// <summary>
        /// Build service provider for Lykke's service.
        /// </summary>
        public static IServiceProvider BuildServiceProvider<TAppSettings>(
            this IServiceCollection services,
            Action<LykkeServiceOptions<TAppSettings>> buildServiceOptions)

            where TAppSettings : BaseAppSettings
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (buildServiceOptions == null)
            {
                throw new ArgumentNullException(nameof(buildServiceOptions));
            }

            var serviceOptions = new LykkeServiceOptions<TAppSettings>();

            buildServiceOptions(serviceOptions);

            if (string.IsNullOrWhiteSpace(serviceOptions.ApiTitle))
            {
                throw new ArgumentException("Api title must be provided.");
            }

            if (serviceOptions.Logs == null)
            {
                throw new ArgumentException("Logs configuration delegate must be provided.");
            }

            var loggingOptions = new LykkeLoggingOptions<TAppSettings>();

            serviceOptions.Logs(loggingOptions);

            if (string.IsNullOrWhiteSpace(loggingOptions.AzureTableName))
            {
                throw new ArgumentException("Logs.AzureTableName must be provided.");
            }

            if (loggingOptions.AzureTableConnectionStringResolver == null)
            {
                throw new ArgumentException("Logs.AzureTableConnectionStringResolver must be provided");
            }

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                })
                .AddFluentValidation(x => x.RegisterValidatorsFromAssembly(Assembly.GetEntryAssembly()));

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", serviceOptions.ApiTitle);

                serviceOptions.Swagger?.Invoke(options);
            });

            var configurationRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var settings = configurationRoot.LoadSettings<TAppSettings>();

            services.AddLykkeLogging(
                settings.ConnectionString(loggingOptions.AzureTableConnectionStringResolver),
                loggingOptions.AzureTableName,
                settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                settings.CurrentValue.SlackNotifications.AzureQueue.QueueName,
                options =>
                {
                    loggingOptions.Extended?.Invoke(options);
                });

            var builder = new ContainerBuilder();

            builder.RegisterInstance(configurationRoot).As<IConfigurationRoot>();            
            builder.RegisterInstance(settings.CurrentValue.SlackNotifications);

            if (settings.CurrentValue.MonitoringServiceClient != null)
                builder.RegisterInstance(settings.Nested(x => x.MonitoringServiceClient));            

            builder.RegisterInstance(serviceOptions);
            builder.RegisterType<AppLifetimeHandler>().AsSelf().SingleInstance();

            builder.Populate(services);
            builder.RegisterAssemblyModules(settings, Assembly.GetEntryAssembly());

            builder.RegisterType<EmptyStartupManager>()
                .As<IStartupManager>()
                .SingleInstance()
                .IfNotRegistered(typeof(IStartupManager));

            builder.RegisterType<EmptyShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance()
                .IfNotRegistered(typeof(IShutdownManager));

            var container = builder.Build();

            var appLifetime = container.Resolve<IApplicationLifetime>();

            appLifetime.ApplicationStarted.Register(container.Resolve<AppLifetimeHandler>().HandleStarted);
            appLifetime.ApplicationStopping.Register(container.Resolve<AppLifetimeHandler>().HandleStopping);
            appLifetime.ApplicationStopped.Register(() => container.Resolve<AppLifetimeHandler>().HandleStopped(container));

            return new AutofacServiceProvider(container);
        }
    }
}
