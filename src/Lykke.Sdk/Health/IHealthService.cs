using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Sdk.Health
{
    /// <summary>
    /// Health service to check the of the service.
    /// </summary>
    /// <seealso href="https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring"/>
    [PublicAPI]
    public interface IHealthService
    {
        /// <summary>
        /// Gets the health violation message.
        /// </summary>
        string GetHealthViolationMessage();

        /// <summary>
        /// Gets the health issues.
        /// </summary>
        IEnumerable<HealthIssue> GetHealthIssues();
    }
}
