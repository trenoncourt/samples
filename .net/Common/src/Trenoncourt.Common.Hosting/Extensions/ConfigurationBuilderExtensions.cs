using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Trenoncourt.Common.Hosting.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDefaultConfiguration(this IConfigurationBuilder builder,
            string environmentName = null)
        {
            string environment = environmentName ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.Equals(environment, EnvironmentName.Development, StringComparison.OrdinalIgnoreCase))
            {
                builder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
            }
            return builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}