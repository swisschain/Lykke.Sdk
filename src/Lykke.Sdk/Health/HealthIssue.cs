using JetBrains.Annotations;

namespace Antares.Sdk.Health
{
    /// <summary>
    /// Health issue model class
    /// </summary>
    [PublicAPI]
    public class HealthIssue
    {
        /// <summary>
        /// The type of health issue.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The value or reason of the health issue.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Create a new health issue
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>the created issue</returns>
        public static HealthIssue Create(string type, string value)
        {
            return new HealthIssue
            {
                Type = type,
                Value = value
            };
        }
    }
}
