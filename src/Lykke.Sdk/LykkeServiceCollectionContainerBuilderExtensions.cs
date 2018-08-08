using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Sdk.ActionFilters;
using Lykke.Sdk.Controllers;
using Lykke.Sdk.Health;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;

namespace Lykke.Sdk
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> class.
    /// </summary>
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

            if (serviceOptions.SwaggerOptions == null)
            {
                throw new ArgumentException("Swagger options must be provided.");
            }

            if (serviceOptions.Logs == null)
            {
                throw new ArgumentException("Logs configuration delegate must be provided.");
            }

            services.AddMvc(options => options.Filters.Add(new ActionValidationFilter()))
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                })
                .AddFluentValidation(x => x.RegisterValidatorsFromAssembly(Assembly.GetEntryAssembly()));

            services.AddTransient<IsAliveController>();

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration(
                    serviceOptions.SwaggerOptions.ApiVersion ?? throw new ArgumentException($"{nameof(LykkeSwaggerOptions)}.{nameof(LykkeSwaggerOptions.ApiVersion)}"),
                    serviceOptions.SwaggerOptions.ApiTitle ?? throw new ArgumentException($"{nameof(LykkeSwaggerOptions)}.{nameof(LykkeSwaggerOptions.ApiTitle)}"));

                serviceOptions.Swagger?.Invoke(options);
            });

            var configurationRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var settings = configurationRoot.LoadSettings<TAppSettings>();

            var appSettings = settings.CurrentValue;
            
            configurationRoot.CheckDependenciesAsync(appSettings, appSettings.SlackNotifications.AzureQueue.ConnectionString,
                appSettings.SlackNotifications.AzureQueue.QueueName, $"{AppEnvironment.Name} {AppEnvironment.Version}");

            var loggingOptions = new LykkeLoggingOptions<TAppSettings>();

            serviceOptions.Logs(loggingOptions);

            if (loggingOptions.HaveToUseEmptyLogging)
            {
                services.AddEmptyLykkeLogging();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(loggingOptions.AzureTableName))
                {
                    throw new ArgumentException("Logs.AzureTableName must be provided.");
                }

                if (loggingOptions.AzureTableConnectionStringResolver == null)
                {
                    throw new ArgumentException("Logs.AzureTableConnectionStringResolver must be provided");
                }
                
                if (settings.CurrentValue.SlackNotifications == null)
                {
                    throw new ArgumentException("SlackNotifications settings section should be specified, when Lykke logging is enabled");
                }

                services.AddLykkeLogging(
                    settings.ConnectionString(loggingOptions.AzureTableConnectionStringResolver),
                    loggingOptions.AzureTableName,
                    settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    settings.CurrentValue.SlackNotifications.AzureQueue.QueueName,
                    options =>
                    {
                        loggingOptions.Extended?.Invoke(options);
                    });
            }

            var builder = new ContainerBuilder();

            serviceOptions.Extend?.Invoke(services, settings);

            builder.RegisterInstance(configurationRoot).As<IConfigurationRoot>();

            if (settings.CurrentValue.MonitoringServiceClient == null)
            {
                throw new InvalidOperationException("MonitoringServiceClient config section is required");
            }

            builder.RegisterInstance(settings.Nested(x => x.MonitoringServiceClient))
                .As<IReloadingManager<MonitoringServiceClientSettings>>();

            builder.RegisterInstance(serviceOptions);
            builder.RegisterType<AppLifetimeHandler>()
                .AsSelf()
                .SingleInstance();

            builder.Populate(services);
            builder.RegisterAssemblyModules(settings, serviceOptions.RegisterAdditionalModules, Assembly.GetEntryAssembly());

            builder.RegisterType<EmptyStartupManager>()
                .As<IStartupManager>()
                .SingleInstance()
                .IfNotRegistered(typeof(IStartupManager));

            builder.RegisterType<EmptyShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance()
                .IfNotRegistered(typeof(IShutdownManager));

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance()
                .IfNotRegistered(typeof(IHealthService));

            var container = builder.Build();

            var appLifetime = container.Resolve<IApplicationLifetime>();

            appLifetime.ApplicationStarted.Register(container.Resolve<AppLifetimeHandler>().HandleStarted);
            appLifetime.ApplicationStopping.Register(container.Resolve<AppLifetimeHandler>().HandleStopping);
            appLifetime.ApplicationStopped.Register(() => container.Resolve<AppLifetimeHandler>().HandleStopped(container));

            return new AutofacServiceProvider(container);
        }
    }
}
