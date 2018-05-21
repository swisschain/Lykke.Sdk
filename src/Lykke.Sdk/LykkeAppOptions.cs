using JetBrains.Annotations;

namespace Lykke.Sdk
{
    [PublicAPI]
    public class LykkeAppOptions
    {
        public string AppName { get; set; }
        public string Version { get; set; }
        public bool IsDebug { get; set; }
    }
}
