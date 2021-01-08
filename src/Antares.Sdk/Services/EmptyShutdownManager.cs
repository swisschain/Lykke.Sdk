using System.Threading.Tasks;

namespace Antares.Sdk.Services
{
    internal class EmptyShutdownManager : IShutdownManager
    {
        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}