using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace Lykke.Sdk
{
    /// <summary>
    /// This class creates IWebHostBuilder which is used in LykkeStarter.
    /// </summary>
    public class WebHostFactory : IWebHostFactory
    {
        /// <inheritdoc />
        public IWebHostBuilder CreateWebHostBuilder<TStartup>(Action<WebHostFactoryOptions> optionConfiguration) 
            where TStartup : class
        {
            if (optionConfiguration == null)
                throw new ArgumentNullException($"{nameof(optionConfiguration)}");

            var options = new WebHostFactoryOptions();
            optionConfiguration(options);

            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{options.Port}")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<TStartup>();

            if (!options.IsDebug)
                hostBuilder = hostBuilder.UseApplicationInsights();

            return hostBuilder;
        }
    }
}
