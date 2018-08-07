using Microsoft.Extensions.Configuration;

namespace Trenoncourt.Common.Hosting.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDefaultConfiguration(this IConfigurationBuilder builder,
            string environmentName) =>
            builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
    }
}