using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace Lykke.Sdk
{
    public static class AutofacExtensions
    {
        public static void RegisterAssemblyModules<TAppSettings>(this ContainerBuilder builder, TAppSettings settings, params Assembly[] assemblies)
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