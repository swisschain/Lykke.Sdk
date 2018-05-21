using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Sdk
{
    [PublicAPI]
    public static class LykkeServiceCollectionContainerBuilderExtensions
    {
        public static IServiceProvider BuildServiceProvider<TAppSettings>(this IServiceCollection services,
            Func<IReloadingManager<TAppSettings>, IReloadingManager<string>> logsConnectionStringFactory)
            where TAppSettings : BaseAppSettings
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new SdkModule<TAppSettings>(logsConnectionStringFactory));
            builder.RegisterAssemblyModules(Assembly.GetCallingAssembly());
            builder.Populate(services);

            var container = builder.Build();

            var appLifetime = container.Resolve<IApplicationLifetime>();
            
            appLifetime.ApplicationStopped.Register(() =>
            {
                container.Dispose();
            });
            
            return new AutofacServiceProvider(container);
        }
    }
}
