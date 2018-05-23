using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Sdk
{
    [PublicAPI]
    public interface IStartupManager
    {
        /// <summary>
        /// Method will be called on IApplicationLifetime.ApplicationStarted event
        /// </summary>
        /// <returns></returns>
        Task StartAsync();
    }
}
