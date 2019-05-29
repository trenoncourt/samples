using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Trenoncourt.Common.Cross.Options;
using Trenoncourt.Common.Hosting.Extensions;

namespace Trenoncourt.Common.Hosting
{
    public static class WebHost
    {
        public static IWebHostBuilder CreateDefaultWebHost()
        {
            IWebHostBuilder webHostBuilder = new WebHostBuilder();
            IConfigurationSection webHostSection = new ConfigurationBuilder().AddDefaultConfiguration().Build().GetSection(nameof(ApplicationOptions.WebHost));
            if (webHostSection?.GetValue<bool>(nameof(ApplicationOptions.WebHost.UseIisIntegration)) ?? false)
            {
                webHostBuilder.UseIISIntegration();
            }
            return webHostBuilder
                .SuppressStatusMessages(webHostSection?.GetValue<bool>(nameof(ApplicationOptions.WebHost.SuppressStatusMessages)) ?? true)
                .UseKestrel((context, options) =>
                {
                    options.AddServerHeader = context.Configuration.GetSection(nameof(ApplicationOptions.Kestrel))
                                                  ?.GetValue<bool>(nameof(ApplicationOptions.Kestrel.AddServerHeader)) ?? false;
                    options.Configure(context.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)));
                    options.ConfigureServerLimits(context);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                    config.AddDefaultConfiguration(hostingContext.HostingEnvironment.EnvironmentName));
        }

        public static IWebHostBuilder CreateDefaultWebHost<TStartup>() where TStartup : class =>
            CreateDefaultWebHost()
                .UseStartup<TStartup>();
    }
}