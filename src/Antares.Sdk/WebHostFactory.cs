using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Antares.Sdk
{
    /// <summary>
    /// This class creates IWebHostBuilder which is used in LykkeStarter.
    /// </summary>
    public class WebHostFactory : IWebHostFactory
    {
        /// <inheritdoc />
        public IHostBuilder CreateWebHostBuilder<TStartup>(Action<WebHostFactoryOptions> optionConfiguration)
            where TStartup : class
        {
            if (optionConfiguration == null)
                throw new ArgumentNullException($"{nameof(optionConfiguration)}");

            var options = new WebHostFactoryOptions();
            optionConfiguration(options);

            var hostBuilder = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseUrls($"http://*:{options.Port}")
                        .UseStartup<TStartup>();

                    if (!options.IsDebug)
                        webBuilder.UseApplicationInsights();
                }).UseServiceProviderFactory(new AutofacServiceProviderFactory());

            return hostBuilder;
        }
    }
}
