using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Antares.Sdk.Services
{
    /// <summary>
    /// Service interface for startup management.
    /// </summary>
    [PublicAPI]
    public interface IStartupManager
    {
        /// <summary>
        /// Method will be called on IApplicationLifetime.ApplicationStarted event
        /// </summary>
        Task StartAsync();
    }
}
