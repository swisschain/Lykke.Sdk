using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Sdk
{
    /// <summary>
    /// Interface for IWebHostBuilder creation. It is exposed for test purposes(TestServer).
    /// </summary>
    public interface IWebHostFactory
    {
        /// <summary>
        /// Creates IWebHostBuilder.
        /// </summary>
        /// <param name="optionConfiguration">Delegate which configures options for IWebHostBuilder.</param>
        /// <typeparam name="TStartup">Startup class</typeparam>
        /// <returns></returns>
        IWebHostBuilder CreateWebHostBuilder<TStartup>(Action<WebHostFactoryOptions> optionConfiguration)
            where TStartup : class;
    }
}
