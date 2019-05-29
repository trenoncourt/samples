using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Trenoncourt.Common.Hosting;
using Trenoncourt.Common.Hosting.Extensions;

namespace Trenoncourt.Common.Api
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(new ConfigurationBuilder().AddDefaultConfiguration().Build())
                    .CreateLogger();
                
                IWebHost webHost = DefaultProgram.LogAndRun(CreateWebHostBuilder);
                Log.Information("Starting web host");
                webHost.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.Information("Closing web host");
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder() =>
            WebHost.CreateDefaultWebHost<Startup>()
                .UseSerilog();
    }
}