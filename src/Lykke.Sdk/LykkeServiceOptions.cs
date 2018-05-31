using System;
using Autofac;
using Lykke.SettingsReader;

namespace Lykke.Sdk
{
    public class LykkeServiceOptions<TAppSettings>
    {
        /// <summary>
        /// Title for Swagger page
        /// </summary>
        public string ApiTitle { get; set; }        
        public Func<IReloadingManager<TAppSettings>, IReloadingManager<string>> LogsConnectionStringFactory { get; set; }
        public string LogsTableName { get; set; }
    }
}