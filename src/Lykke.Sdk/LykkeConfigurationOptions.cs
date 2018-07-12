using System;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Microsoft.AspNetCore.Builder;

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

        /// <summary>Lykke swagger options</summary>
        public LykkeSwaggerOptions SwaggerOptions { get; set; }

        /// <summary>Additional middleware</summary>
        public Action<IApplicationBuilder> WithMiddleware { get; set; }

        internal LykkeConfigurationOptions()
        {
            DefaultErrorHandler = ex => ErrorResponse.Create("Technical problem");
            SwaggerOptions = new LykkeSwaggerOptions();
        }

    }
}
