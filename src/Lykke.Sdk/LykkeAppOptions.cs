using JetBrains.Annotations;

namespace Lykke.Sdk
{
    [PublicAPI]
    public class LykkeAppOptions
    {
        public string AppName { get; set; } = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
        public string ApiVersion { get; set; }
        public bool IsDebug { get; set; }
    }
}
