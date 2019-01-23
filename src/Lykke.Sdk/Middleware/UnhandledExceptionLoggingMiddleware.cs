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
    public class UnhandledExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _log;

        /// <summary>
        /// Middleware that handles all unhandled exceptions and logs them as errors.
        /// </summary>
        public UnhandledExceptionLoggingMiddleware(RequestDelegate next, ILogFactory logFactory)
        {
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            _next = next;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(context, ex);
                throw;
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