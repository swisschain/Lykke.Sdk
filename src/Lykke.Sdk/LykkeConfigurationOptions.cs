using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;

namespace Lykke.Sdk
{
    /// <summary>
    /// Configuration options used in <see cref="LykkeApplicationBuilderExtensions.UseLykkeConfiguration"/> extension method.
    /// </summary>
    [PublicAPI]
    public class LykkeConfigurationOptions
    {
        /// <summary>Default error handler.</summary>
        public CreateErrorResponse DefaultErrorHandler { get; set; }

        /// <summary>API version</summary>
        public string ApiVersion { get; set; }

        /// <summary>Document title displayed in Swagger UI.</summary>
        public string SwaggerDocumentTitle { get; set; }

        internal LykkeConfigurationOptions()
        {
            DefaultErrorHandler = ex => ErrorResponse.Create("Technical problem");
            ApiVersion = "v1";
        }
    }
}
