// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Host;
using ProcessManagerSample.MessageMiddlewares;
using ProcessManagerSample.Queries;
using System.Reflection;

namespace ProcessManagerSample
{
    public class Startup
    {
        public static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetEntryAssembly());

            services.AddMessageBus().AddInProcessTransport();

            services.AddMediatR(typeof(GetPartnerQuery).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            //services.AddScoped<INotificationHandler<TimeoutOccured>, TimeoutOccuredHandler>();

            services.AddProcessManager(Assembly.GetEntryAssembly());

            services.AddEventStore(es =>
            {
                es.UseNewtownsoftJson();
                es.UseInMemoryEventRepository();
            });

            services.AddMessagingHost(
                context.Configuration,
                hostBuilder => hostBuilder
                .Configure(configBuilder => configBuilder
                    .AddSubscriberServices(subscriberBulder => subscriberBulder
                        .FromMediatRHandledCommands().AddAllClasses()
                        .FromMediatRHandledEvents().AddAllClasses())
                    .WithDefaultOptions()
                    .UsePipeline(builder => builder
                        .UseCorrelationMiddleware()
                        .UseExceptionHandlingMiddleware()
                        .UseOpenTracingMiddleware()
                        .UseDefaultResiliencyMiddleware()
                        .UseMiddleware<SubscriberLoggingMiddleware>()
                        .UseMediatRMiddleware())
                )
            );
        }
    }
}
