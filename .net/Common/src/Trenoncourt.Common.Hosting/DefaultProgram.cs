using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Trenoncourt.Common.Hosting.Extensions;

namespace Trenoncourt.Common.Hosting
{
    public static class DefaultProgram
    {
        public static IWebHost LogAndRun(Func<IWebHostBuilder> webHostBuilder)
        {
            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            
            string urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            Log.Information("Starting application with environment {environmentName} in directory {currentDirectory}, listening at {urls}", environmentName, Directory.GetCurrentDirectory(), urls);
            
            Log.Information($"Creating & building web host");
            return webHostBuilder.Invoke().Build();
    
        }
    }
}