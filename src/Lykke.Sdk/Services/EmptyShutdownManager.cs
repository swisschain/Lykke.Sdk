using System.Threading.Tasks;

namespace Lykke.Sdk
{
    internal class EmptyShutdownManager : IShutdownManager
    {
        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}