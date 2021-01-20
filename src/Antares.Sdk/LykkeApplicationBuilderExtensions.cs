using System;
using System.Linq;
using Antares.Sdk.Middleware;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Swisschain.Sdk.Metrics.Rest;

namespace Antares.Sdk
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> class.
    /// </summary>
    [PublicAPI]
    public static class LykkeApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure Lykke service.
        /// </summary>
        /// <param name="app"></param>
        public static IApplicationBuilder UseLykkeConfiguration(this IApplicationBuilder app)
            => app.UseLykkeConfiguration(null);

        /// <summary>
        /// Configure Lykke service.
        /// </summary>
        /// <param name="app">IApplicationBuilder implementation.</param>
        /// <param name="configureOptions">Configuration handler for <see cref="LykkeConfigurationOptions"/></param>
        public static IApplicationBuilder UseLykkeConfiguration(this IApplicationBuilder app, Action<LykkeConfigurationOptions> configureOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new LykkeConfigurationOptions();
            configureOptions?.Invoke(options);

            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();

            try
            {
                app.UseMiddleware<PrometheusMetricsMiddleware>();

                app.UseMiddleware<UnhandledExceptionResponseMiddleware>(
                    options.DefaultErrorHandler,
                    options.UnhandledExceptionHttpStatusCodeResolver);

                if (!options.HaveToDisableUnhandledExceptionLoggingMiddleware)
                {
                    app.UseMiddleware<UnhandledExceptionLoggingMiddleware>();
                }

                if (!options.HaveToDisableValidationExceptionMiddleware)
                {
                    app.UseMiddleware<ClientServiceApiExceptionMiddleware>();
                }

                app.UseLykkeForwardedHeaders();

                app.UseStaticFiles();
                app.UseRouting();

                // Middleware like authentication needs to be registered before endpoints
                options.WithMiddleware?.Invoke(app);

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapGrpcReflectionService();

                    options.RegisterEndpoints?.Invoke(endpoints);
                });

                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint($"/swagger/{options.SwaggerOptions.ApiVersion}/swagger.json", options.SwaggerOptions.ApiVersion);

                    if (options.AdditionalSwaggerOptions.Any())
                    {
                        foreach (var swaggerVersion in options.AdditionalSwaggerOptions)
                        {
                            if (string.IsNullOrEmpty(swaggerVersion.ApiVersion))
                                throw new ArgumentNullException($"{nameof(options.AdditionalSwaggerOptions)}.{nameof(LykkeSwaggerOptions.ApiVersion)}");

                            x.SwaggerEndpoint($"/swagger/{swaggerVersion.ApiVersion}/swagger.json", swaggerVersion.ApiVersion);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(options.SwaggerOptions.ApiTitle))
                    {
                        x.DocumentTitle = options.SwaggerOptions.ApiTitle;
                    }
                });
            }
            catch (Exception ex)
            {
                try
                {
                    var log = app.ApplicationServices.GetService<ILogFactory>().CreateLog(typeof(LykkeApplicationBuilderExtensions).FullName);

                    log.Critical(ex);
                }
                catch (Exception ex1)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine(ex1);
                }

                throw;
            }

            return app;
        }
    }
}
