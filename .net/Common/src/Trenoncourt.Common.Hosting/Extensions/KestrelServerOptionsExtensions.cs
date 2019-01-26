using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Trenoncourt.Common.Cross.Options;

namespace Trenoncourt.Common.Hosting.Extensions
{
    public static class KestrelServerOptionsExtensions
    {
        /// <summary>
        /// Configure Kestrel server limits from appsettings.json is not supported. So we manually copy them from config.
        /// See https://github.com/aspnet/KestrelHttpServer/issues/2216
        /// </summary>
        public static void ConfigureServerLimits(this KestrelServerOptions options, WebHostBuilderContext builderContext)
        {
            var kestrelOptions = new KestrelServerOptions();
            builderContext.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)).Bind(kestrelOptions);
            foreach (var property in typeof(KestrelServerLimits).GetProperties())
            {
                if (property.PropertyType == typeof(MinDataRate))
                {
                    var section = builderContext.Configuration.GetSection(
                        $"{nameof(ApplicationOptions.Kestrel)}:{nameof(KestrelServerOptions.Limits)}");
                    if (section.GetChildren().Any(x => string.Equals(x.Key, property.Name, StringComparison.Ordinal)))
                    {
                        var bytesPerSecond = section.GetValue<double>($"{property.Name}:{nameof(MinDataRate.BytesPerSecond)}");
                        var gracePeriod = section.GetValue<TimeSpan>($"{property.Name}:{nameof(MinDataRate.GracePeriod)}");
                        property.SetValue(options.Limits, new MinDataRate(bytesPerSecond, gracePeriod));
                    }
                }
                else
                {
                    var value = property.GetValue(kestrelOptions.Limits);
                    property.SetValue(options.Limits, value);
                }
            }
        }
    }
}