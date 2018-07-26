using System.Threading.Tasks;

namespace Lykke.Sdk
{
    internal class EmptyStartupManager : IStartupManager
    {
        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}