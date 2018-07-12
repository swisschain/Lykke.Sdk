using JetBrains.Annotations;

namespace Lykke.Sdk.Health
{
    /// <summary>
    /// Maintenance mode indicator.
    /// </summary>
    [PublicAPI]
    public class MaintenanceMode
    {
        /// <summary>
        /// Flag indicating whether <see cref="MaintenanceMode"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Reason for the maintenance mode.
        /// </summary>
        public string Reason { get; set; }
    }
}