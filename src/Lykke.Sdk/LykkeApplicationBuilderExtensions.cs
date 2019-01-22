using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Sdk
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

            var env = app.ApplicationServices.GetService<IHostingEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            try
            {
                app.UseLykkeMiddleware(options.DefaultErrorHandler);
                app.UseMiddleware<ClientServiceApiExceptionMiddleware>();
                app.UseLykkeForwardedHeaders();

                // Middleware like authentication needs to be registered before Mvc
                options.WithMiddleware?.Invoke(app);

                app.UseStaticFiles();
                app.UseMvc();

                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint($"/swagger/{options.SwaggerOptions.ApiVersion}/swagger.json", options.SwaggerOptions.ApiVersion);

                    if (options.AdditionalSwaggerOptions.Any())
                    {
                        foreach (var swaggerVersion in options.AdditionalSwaggerOptions)
                        {
                            if (string.IsNullOrEmpty(swaggerVersion.ApiVersion))
                                throw new ArgumentException($"{nameof(options.AdditionalSwaggerOptions)}.{nameof(LykkeSwaggerOptions.ApiVersion)}");
                            
                            x.SwaggerEndpoint($"/swagger/{swaggerVersion.ApiVersion}/swagger.json", swaggerVersion.ApiVersion);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(options.SwaggerOptions.ApiTitle))
                    {
                        x.DocumentTitle(options.SwaggerOptions.ApiTitle);
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
