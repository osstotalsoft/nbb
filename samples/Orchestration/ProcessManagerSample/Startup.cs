﻿using AutoMapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Core.Abstractions;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.InProcessMessaging.Extensions;
using NBB.ProcessManager.Runtime;
using NBB.Resiliency;
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
            //services.AddNatsMessaging();
            services.AddInProcessMessaging();

            services.AddMediatR(typeof(GetPartnerQuery).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            //services.AddScoped<INotificationHandler<TimeoutOccured>, TimeoutOccuredHandler>();

            services.AddProcessManager(Assembly.GetEntryAssembly());

            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer()
                .WithAdoNetEventRepository();

            services.AddResiliency();
            services.AddMessagingHost()
                .AddSubscriberServices(config => config
                    .FromMediatRHandledCommands().AddClassesAssignableTo<ICommand>()
                    .FromMediatRHandledEvents().AddAllClasses())
                .WithDefaultOptions()
                .UsePipeline(builder => builder
                    .UseCorrelationMiddleware()
                    .UseExceptionHandlingMiddleware()
                    //.UseMiddleware<OpenTracingMiddleware>()
                    .UseDefaultResiliencyMiddleware()
                    .UseMiddleware<SubscriberLoggingMiddleware>()
                    .UseMediatRMiddleware());
        }
    }
}