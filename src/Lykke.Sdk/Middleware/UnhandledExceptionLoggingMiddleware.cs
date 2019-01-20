using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace Lykke.Sdk.Middleware
{
    /// <summary>
    /// Middleware that handles all unhandled exceptions and logs them as errors.
    /// </summary>
    [PublicAPI]
    public class UnhandledExceptionLoggingMiddleware : IMiddleware
    {
        private readonly ILog _log;

        /// <summary>
        /// Middleware that handles all unhandled exceptions and logs them as errors.
        /// </summary>
        public UnhandledExceptionLoggingMiddleware(ILogFactory logFactory)
        {
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            _log = logFactory.CreateLog(this);
        }

        async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(context, ex);
            }
        }

        private async Task LogErrorAsync(HttpContext context, Exception ex)
        {
            var url = context.Request?.GetUri()?.AbsoluteUri;
            var urlWithoutQuery = GetUrlWithoutQuery(url) ?? "?";
            var body = await GetPartialRequestBodyAsync(context.Request);

            _log.Error(
                urlWithoutQuery,
                ex,
                null,
                new
                {
                    Url = url,
                    Body = body
                });
        }

        private static async Task<string> GetPartialRequestBodyAsync(HttpRequest request)
        {
            if (request?.Body == null)
            {
                return null;
            }

            // request body might be already read at the moment 
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
            }
            
            // Set limit to 64 Kb.
            const int maxBytesToRead = 1024 * 64;
            var bodyBytes = new byte[maxBytesToRead];
            var bodyBytesCount = await request.Body.ReadAsync(bodyBytes, 0, maxBytesToRead);

            return Encoding.UTF8.GetString(bodyBytes, 0, bodyBytesCount);
        }

        [CanBeNull]
        private static string GetUrlWithoutQuery([CanBeNull] string url)
        {
            if (url == null)
            {
                return null;
            }

            var index = url.IndexOf('?');
            var urlWithoutQuery = index == -1 ? url : url.Substring(0, index);

            return urlWithoutQuery;
        }
    }
}