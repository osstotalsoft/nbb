﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Abstractions;
using ProcessManagerSample.Events;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace ProcessManagerSample
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                var host = CreateHost(args);

                using (host)
                {
                    await host.StartAsync();


                    Console.ReadKey();
                    var orderId = Guid.Empty;
                    var pub = host.Services.GetRequiredService<IMessageBusPublisher>();
                    await pub.PublishAsync(new OrderCreated(orderId, 100, 0,0));
                    Console.ReadKey();
                    await pub.PublishAsync(new OrderPaymentCreated(orderId, 100, 0,0));
                    await pub.PublishAsync(new OrderPaymentReceived(orderId, 0, 0));
                    Console.ReadKey();
                    await pub.PublishAsync(new OrderShipped(orderId, 0, 0));


                    await host.WaitForShutdownAsync();
                }

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

        public static IHost CreateHost(string[] args) =>
            new HostBuilder()
                .ConfigureHostConfiguration(builder => { builder.AddEnvironmentVariables(); })
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("environment") ?? "Production"}.json", optional: true)
                        .AddEnvironmentVariables();

                    if (hostBuilderContext.HostingEnvironment.IsDevelopment())
                    {
                        configurationBuilder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices(Startup.ConfigureServicesDelegate)
                .UseSerilog()
                .Build();
    }
}