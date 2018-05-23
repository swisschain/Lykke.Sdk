using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Sdk
{
    [PublicAPI]
    public interface IShutdownManager
    {
        /// <summary>
        /// Method will be called on IApplicationLifetime.ApplicationStopping event
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}