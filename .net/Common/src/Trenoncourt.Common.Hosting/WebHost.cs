using System.IO;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Trenoncourt.Common.Hosting.Extensions;

namespace Trenoncourt.Common.Hosting
{
    public static class WebHost
    {
        public static IWebHostBuilder CreateDefaultWebHost() =>
            new WebHostBuilder()
                .SuppressStatusMessages(true)
                .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                    config.AddDefaultConfiguration(hostingContext.HostingEnvironment.EnvironmentName));
        
        public static IWebHostBuilder CreateDefaultWebHost<TStartup>() where TStartup : class =>
            CreateDefaultWebHost()
                .UseStartup<IStartup>();
    }
}