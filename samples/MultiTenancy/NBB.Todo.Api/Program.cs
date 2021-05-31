using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace NBB.Todo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            Log.Information("Starting NBB.Todo.Api");

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureLogging(ConfigureLogging)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void ConfigureLogging(HostBuilderContext ctx, ILoggingBuilder _builder)
        {
            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(ctx.Configuration)
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();
        }
    }
}
