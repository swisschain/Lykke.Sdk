using System;
using Autofac;
using Lykke.SettingsReader;

namespace Lykke.Sdk
{
    public class LykkeServiceOptions
    {
        /// <summary>
        /// Version of api, e.g. 'v1'
        /// </summary>
        public string ApiVersion { get; set; }
        /// <summary>
        /// Title for Swagger page
        /// </summary>
        public string ApiTitle { get; set; }        
        public Func<IComponentContext, IReloadingManager<string>> LogsConnectionStringFactory { get; set; }
        public string LogsTableName { get; set; }
    }
}