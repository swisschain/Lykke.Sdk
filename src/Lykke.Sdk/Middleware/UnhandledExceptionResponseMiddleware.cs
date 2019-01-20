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
    /// Delegate to resolve HTTP status code in case of unhandled exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    [PublicAPI]
    public delegate HttpStatusCode ResolveHttpStatusCode(Exception ex);

    /// <summary>
    /// Middleware that handles all unhandled exceptions and uses delegate to generate the error response object.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    internal class UnhandledExceptionResponseMiddleware : IMiddleware
    {
        private readonly CreateErrorResponse _errorResponseFactory;
        private readonly ResolveHttpStatusCode _httpStatusCodeResolver;

        /// <summary>
        /// Middleware that handles all unhandled exceptions and uses delegate to generate the error response object.
        /// </summary>
        public UnhandledExceptionResponseMiddleware(
            CreateErrorResponse errorResponseFactory,
            ResolveHttpStatusCode httpStatusCodeResolver)
        {
            _errorResponseFactory = errorResponseFactory ?? throw new ArgumentNullException(nameof(errorResponseFactory));
            _httpStatusCodeResolver = httpStatusCodeResolver ?? throw new ArgumentNullException(nameof(httpStatusCodeResolver));
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
                context.Response.StatusCode = (int) _httpStatusCodeResolver.Invoke(ex);

                var response = _errorResponseFactory.Invoke(ex);
                var responseJson = JsonConvert.SerializeObject(response);

                await context.Response.WriteAsync(responseJson);
            }
        }        
    }
}