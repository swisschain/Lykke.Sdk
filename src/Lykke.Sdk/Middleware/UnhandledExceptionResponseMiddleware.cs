using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Lykke.Sdk.Middleware
{
    /// <summary>
    /// Middleware that handles all unhandled exceptions and uses delegate to generate the error response object.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    internal class UnhandledExceptionResponseMiddleware : IMiddleware
    {
        private readonly CreateErrorResponse _createErrorResponse;

        /// <summary>
        /// Middleware that handles all unhandled exceptions and uses delegate to generate the error response object.
        /// </summary>
        public UnhandledExceptionResponseMiddleware(CreateErrorResponse errorResponseFactory)
        {
            _createErrorResponse = errorResponseFactory ?? throw new ArgumentNullException(nameof(errorResponseFactory));
        }

        async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                var response = _createErrorResponse(ex);
                var responseJson = JsonConvert.SerializeObject(response);

                await context.Response.WriteAsync(responseJson);
            }
        }        
    }
}