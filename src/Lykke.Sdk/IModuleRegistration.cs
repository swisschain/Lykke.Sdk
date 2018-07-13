using Autofac.Core;
using JetBrains.Annotations;
using Lykke.SettingsReader;

namespace Lykke.Sdk
{
    /// <summary>
    /// Fluent interface for registration of additional autofac modules outside the entry assembly.
    /// </summary>
    /// <remarks>Modules in the entry assembly are loaded automatically</remarks>
    [PublicAPI]
    public interface IModuleRegistration
    {
        /// <summary>
        /// Registers an additional AutoFac module.
        /// </summary>
        /// <typeparam name="TModule">The type of the module.</typeparam>
        /// <returns>this</returns>
        /// <remarks>when you want to inject settings in your module add <see cref="IReloadingManager{TAppSettings}"/> to your constructor</remarks>
        IModuleRegistration RegisterModule<TModule>()
            where TModule : IModule;
    }
}