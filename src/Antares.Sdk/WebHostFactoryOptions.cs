namespace Antares.Sdk
{
    public class WebHostFactoryOptions
    {
        public bool IsDebug { get; set; }

        public int Port { get; set; }

        public int GrpcPort { get; set; }

        internal WebHostFactoryOptions()
        {
            Port = 5000;
            GrpcPort = 5001;
        }
    }
}