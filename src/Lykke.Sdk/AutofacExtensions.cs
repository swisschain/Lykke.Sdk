using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Common.Log;
using Lykke.SettingsReader;

namespace Lykke.Sdk
{
    public static class AutofacExtensions
    {
        internal static void RegisterAssemblyModules<TAppSettings>(this ContainerBuilder builder, IReloadingManager<TAppSettings> settings, ILog logger, params Assembly[] assemblies)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            var moduleType = typeof(Autofac.Module);
            var internalBuilder = new ContainerBuilder();

            internalBuilder
                .RegisterAssemblyTypes(assemblies)
                .Where(t => moduleType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                .WithParameter(TypedParameter.From(settings))
                .WithParameter(TypedParameter.From(logger))
                .As<IModule>();

            using (var ctx = internalBuilder.Build())
            {
                foreach (var module in ctx.Resolve<IEnumerable<IModule>>())
                {
                    builder.RegisterModule(module);
                }
            }
        }
    }
}