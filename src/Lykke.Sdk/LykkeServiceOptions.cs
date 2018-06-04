using System;

namespace Lykke.Sdk
{
    public class LykkeServiceOptions<TAppSettings>
    {
        /// <summary>
        /// Title for Swagger page
        /// </summary>
        public string ApiTitle { get; set; }        
        public (string TableName, Func<TAppSettings, string> ConnectionString) Logs { get; set; }
    }
}