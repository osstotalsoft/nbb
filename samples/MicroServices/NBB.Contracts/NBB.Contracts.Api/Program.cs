// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.Serilog;
using NBB.Tools.Serilog.OpenTracingSink;
using Serilog;

namespace NBB.Contracts.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, logConfig) =>
                {
                    logConfig
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {TenantCode:u}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.OpenTracing(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
                })
                //.ConfigureLogging(config =>
                //{
                //
                //    config.AddOpenTelemetry(options =>
                //    {
                //        options.IncludeFormattedMessage = true;
                //        //options.IncludeScopes = true;
                //        //options.ParseStateValues = true;
                //        options.AttachLogsToActivityEvent(); // <PackageReference Include="OpenTelemetry.Extensions" Version="1.0.0-beta.3" />
                //        options.AddConsoleExporter();                        
                //    });
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
