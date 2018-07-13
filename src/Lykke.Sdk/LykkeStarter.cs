using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Sdk
{
    /// <summary>
    /// Lykke default startup routine
    /// </summary>
    [PublicAPI]
    public static class LykkeStarter
    {
        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="componentName">Name of the component.</param>
        public static async Task Start<TStartup>(string componentName)
            where TStartup : class
        {
            Console.WriteLine($@"{componentName} version {
                    Common.AppEnvironment.Version
                }");

            if (Debugger.IsAttached)
            {
                Console.WriteLine(@"Is DEBUG");
            }
            else
            {
                Console.WriteLine(@"Is RELEASE");
            }

            Console.WriteLine($@"ENV_INFO: {Common.AppEnvironment.EnvInfo}");

            try
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls("http://*:5000")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<TStartup>()
                    .UseApplicationInsights()
                    .Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($@"Process will be terminated in {delay}. Press any key to terminate immediately.");

                await Task.WhenAny(
                    Task.Delay(delay),
                    Task.Run(() => Console.ReadKey(true)));
            }

            Console.WriteLine(@"Terminated");
        }
    }
}
