using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NBB.Correlation.Serilog;
using Serilog;
using Serilog.Events;

namespace NBB.Mono
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");


                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString, "Logs", autoCreateSqlTable: true)
                        .CreateLogger();


                    //logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    //logging.AddConsole();
                    //logging.AddDebug();
                    //logging.AddSerilog(dispose: true);
                })
                .UseStartup<Startup>()
                //.UseSerilog()
                .Build();
    }
}
