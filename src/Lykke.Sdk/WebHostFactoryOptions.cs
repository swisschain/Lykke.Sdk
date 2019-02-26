namespace Lykke.Sdk
{
    public class WebHostFactoryOptions
    {
        public bool IsDebug { get; set; }

        public int Port { get; set; }

        internal WebHostFactoryOptions()
        {
            Port = 5000;
        }
    }
}