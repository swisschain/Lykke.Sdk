using System;
using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Logs;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;

namespace Lykke.Sdk
{
    internal class SdkModule : Module
    {
        private readonly string _logsTableName;
        private readonly Func<IComponentContext, IReloadingManager<string>> _logsConnectionStringFactory;
        
        public SdkModule(Func<IComponentContext, IReloadingManager<string>> logsConnectionStringFactory, string logsTableName)
        {
            _logsTableName = logsTableName ?? throw new ArgumentNullException("logsTableName");            
            _logsConnectionStringFactory = logsConnectionStringFactory ?? throw new ArgumentNullException("logsConnectionStringFactory");
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx =>
                {
                    return CreateLogWithSlack(
                        _logsTableName,
                        builder,
                        _logsConnectionStringFactory(ctx),
                        ctx.Resolve<SlackNotificationsSettings>());
                })
                .As<ILog>()
                .SingleInstance();
        }

        private static ILog CreateLogWithSlack(string logsTableName, ContainerBuilder builder, IReloadingManager<string> logsConnectionString, SlackNotificationsSettings slackNotificationsSettings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);
            
            var dbLogConnectionString = logsConnectionString.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                consoleLogger.WriteWarning("SdkModule", nameof(CreateLogWithSlack), "Table loggger is not inited");
                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(logsConnectionString, logsTableName, consoleLogger),
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
