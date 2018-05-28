using System;
using Autofac;
using Lykke.SettingsReader;

namespace Lykke.Sdk
{
    public class LykkeServiceOptions
    {
        public string ApiVersion { get; set; }
        public string ApiTitle { get; set; }
        public Func<IComponentContext, IReloadingManager<string>> LogsConnectionStringFactory { get; set; }
        public string LogsTableName { get; set; }
    }
}