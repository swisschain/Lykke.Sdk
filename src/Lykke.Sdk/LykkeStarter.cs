using JetBrains.Annotations;
using Lykke.Common;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
        public static Task Start<TStartup>()
            where TStartup : class
        {
            return Start<TStartup>(AppEnvironment.Name, 5000);
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="componentName">Name of the component.</param>
        public static Task Start<TStartup>(string componentName)
            where TStartup : class
        {
            return Start<TStartup>(componentName, 5000);
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="port">Port that the app is listening to.</param>
        public static async Task Start<TStartup>(string componentName, int port)
            where TStartup : class
        {
            Console.WriteLine($@"{componentName} version {AppEnvironment.Version}");
            Console.WriteLine(Debugger.IsAttached ? "DEBUG mode" : "RELEASE mode");
            Console.WriteLine($@"ENV_INFO: {AppEnvironment.EnvInfo}");

            try
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls($"http://*:{port}")
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
