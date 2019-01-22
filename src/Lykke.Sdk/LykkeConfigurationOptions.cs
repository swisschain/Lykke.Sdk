using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Microsoft.AspNetCore.Builder;

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

        /// <summary>Lykke swagger options</summary>
        public LykkeSwaggerOptions SwaggerOptions { get; set; }

        /// <summary>Additional lykke swagger options</summary>
        public IReadOnlyCollection<LykkeSwaggerOptions> AdditionalSwaggerOptions { get; set; } =
            Array.Empty<LykkeSwaggerOptions>();

        /// <summary>Additional middleware</summary>
        public Action<IApplicationBuilder> WithMiddleware { get; set; }

        internal LykkeConfigurationOptions()
        {
            DefaultErrorHandler = ex => ErrorResponse.Create("Technical problem");
            SwaggerOptions = new LykkeSwaggerOptions();
        }

    }
}
