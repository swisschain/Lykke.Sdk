using System.Threading.Tasks;

namespace Antares.Sdk.Services
{
    internal class EmptyStartupManager : IStartupManager
    {
        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}