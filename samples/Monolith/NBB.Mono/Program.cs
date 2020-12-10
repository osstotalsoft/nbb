using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace NBB.Mono
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
                        .CreateLogger();


                    //logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    //logging.AddConsole();
                    //logging.AddDebug();
                    //logging.AddSerilog(dispose: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
