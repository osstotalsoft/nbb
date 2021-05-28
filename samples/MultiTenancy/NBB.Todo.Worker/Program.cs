using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;

namespace NBB.Todo.Worker
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static async Task<int> Main(string[] args)
        {
            try
            {
                var host = BuildWebHost(args);
                Log.Information("Starting NBB.Tasks.Worker");
                Log.Information("Messaging.Env=" + Configuration.GetSection("Messaging")["Env"]);

                await host.RunAsync(CancellationToken.None);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cb => BuildConfiguration(cb))
                .UseStartup<Startup>()
                //.UseSerilog()
                .Build();

        private static void BuildConfiguration(IConfigurationBuilder configurationBuilder)
        {
            Configuration = configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
