using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;

namespace Lykke.Sdk
{
    /// <summary>
    /// Configuration options used in UseLykkeConfiguration extension method.
    /// </summary>
    [PublicAPI]
    public class LykkeConfigurationOptions
    {
        /// <summary>Default error handler.</summary>
        public CreateErrorResponse DefaultErrorHandler { get; set; }

        internal LykkeConfigurationOptions()
        {
            DefaultErrorHandler = ex => ErrorResponse.Create("Technical problem");
        }
    }
}
