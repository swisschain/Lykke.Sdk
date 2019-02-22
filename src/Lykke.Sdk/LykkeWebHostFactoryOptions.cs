namespace Lykke.Sdk
{
    public class LykkeWebHostFactoryOptions
    {
        public bool IsDebug { get; set; }

        public int Port { get; set; }

        internal LykkeWebHostFactoryOptions()
        {
            Port = 5000;
        }
    }
}