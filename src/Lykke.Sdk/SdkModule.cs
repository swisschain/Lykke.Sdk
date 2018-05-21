using System;
using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Logs;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lykke.Sdk
{
    internal class SdkModule<TAppSettings> : Module 
        where TAppSettings : BaseAppSettings
    {
        private readonly Func<IReloadingManager<TAppSettings>, IReloadingManager<string>> _logsConnectionStringFactory;
        
        public SdkModule(Func<IReloadingManager<TAppSettings>,IReloadingManager<string>> logsConnectionStringFactory)
        {
            _logsConnectionStringFactory = logsConnectionStringFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx => new ConfigurationBuilder().AddEnvironmentVariables().Build())
                .As<IConfigurationRoot>().SingleInstance();

            builder.Register(ctx => ctx.Resolve<IConfigurationRoot>().LoadSettings<TAppSettings>())
                .As<IReloadingManager<TAppSettings>>()
                .SingleInstance();

            builder.Register(ctx =>
            {
                var settings = ctx.Resolve<IReloadingManager<TAppSettings>>();

                return settings.Nested(x => x.MonitoringServiceClient);
            })
            .As<IReloadingManager<MonitoringServiceClientSettings>>();

            builder.Register(ctx =>
                {
                    var settings = ctx.Resolve<IReloadingManager<TAppSettings>>();

                    return CreateLogWithSlack(
                        builder,
                        _logsConnectionStringFactory(settings),
                        settings.Nested(x => x.SlackNotifications).CurrentValue);
                })
                .As<ILog>()
                .SingleInstance();
        }

        private static ILog CreateLogWithSlack(ContainerBuilder builder, IReloadingManager<string> logsConnectionString, SlackNotificationsSettings slackNotificationsSettings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);
            
            var dbLogConnectionString = logsConnectionString.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                consoleLogger.WriteWarningAsync("SdkModule", nameof(CreateLogWithSlack), "Table loggger is not inited").Wait();
                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(logsConnectionString, "LykkeServiceLog", consoleLogger),
                consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = builder.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = slackNotificationsSettings.AzureQueue.ConnectionString,
                QueueName = slackNotificationsSettings.AzureQueue.QueueName
            }, aggregateLogger);

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

            // Creating azure storage logger, which logs own messages to concole log
            var azureStorageLogger = new LykkeLogToAzureStorage(
                persistenceManager,
                slackNotificationsManager,
                consoleLogger);

            azureStorageLogger.Start();

            aggregateLogger.AddLog(azureStorageLogger);

            return aggregateLogger;
        }
    }    
}
