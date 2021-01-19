using System;
using System.IO;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
                    webBuilder.UseKestrel(kestrelOptions =>
                        {
                            Console.WriteLine($"Options - HttpPort: {options.Port}");
                            Console.WriteLine($"Options - GrpcPort: {options.GrpcPort}");

                            kestrelOptions.Listen(IPAddress.Any, options.Port, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1;
                            });

                            kestrelOptions.Listen(IPAddress.Any, options.GrpcPort, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                        .UseStartup<TStartup>();

                    if (!options.IsDebug)
                        webBuilder.UseApplicationInsights();
                }).UseServiceProviderFactory(new AutofacServiceProviderFactory());

            return hostBuilder;
        }
    }
}
