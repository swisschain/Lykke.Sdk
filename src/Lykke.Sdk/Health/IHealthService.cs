using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Sdk.Health
{
    [PublicAPI]
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public interface IHealthService
    {
        string GetHealthViolationMessage();
        IEnumerable<HealthIssue> GetHealthIssues();
    }
}
