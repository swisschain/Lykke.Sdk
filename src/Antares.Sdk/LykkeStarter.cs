using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common;
using Microsoft.Extensions.Hosting;

namespace Antares.Sdk
{
    /// <summary>
    /// Lykke default startup routine
    /// </summary>
    [PublicAPI]
    public static class LykkeStarter
    {
        private static object _locker = new object();
        private static IWebHostFactory _webHostBuilderFactory = new WebHostFactory();

        /// <summary>DEBUG/RELEASE mode flag.</summary>
        public static bool IsDebug { get; private set; }

        /// <summary>WebHostFactory for creating IWebHostBuilder.</summary>
        public static IWebHostFactory WebHostFactory
        {
            get => _webHostBuilderFactory;
            set => _webHostBuilderFactory =
                    value ?? throw new ArgumentNullException($"{nameof(WebHostFactory)} can't be null");
        }

        /// <summary>Starts the service.</summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="isDebug">DEBUG/RELEASE mode flag</param>
        public static Task Start<TStartup>(bool isDebug)
            where TStartup : class
        {
            return Start<TStartup>(isDebug, 5000);
        }

        /// <summary>Starts the service listening to provided port.</summary>
        /// <typeparam name="TStartup">The type of the startup.</typeparam>
        /// <param name="port">Port that the app is listening to.</param>
        /// /// <param name="isDebug">DEBUG/RELEASE mode flag</param>
        public static async Task Start<TStartup>(bool isDebug, int port)
            where TStartup : class
        {
            IsDebug = isDebug;

            Console.WriteLine($@"{AppEnvironment.Name} version {AppEnvironment.Version}");
            Console.WriteLine(isDebug ? "DEBUG mode" : "RELEASE mode");
            Console.WriteLine($@"ENV_INFO: {AppEnvironment.EnvInfo}");

            try
            {
                var hostBuilder = _webHostBuilderFactory
                    .CreateWebHostBuilder<TStartup>(options =>
                {
                    options.Port = port;
                    options.IsDebug = isDebug;
                });

                var host = hostBuilder.Build();

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
