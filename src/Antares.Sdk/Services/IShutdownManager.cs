using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Antares.Sdk.Services
{
    /// <summary>
    /// Service interface for shutdown management.
    /// </summary>
    [PublicAPI]
    public interface IShutdownManager
    {
        /// <summary>
        /// Method will be called on IApplicationLifetime.ApplicationStopping event
        /// </summary>
        Task StopAsync();
    }
}