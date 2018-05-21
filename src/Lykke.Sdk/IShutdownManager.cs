using System.Threading.Tasks;

namespace Lykke.Sdk
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}