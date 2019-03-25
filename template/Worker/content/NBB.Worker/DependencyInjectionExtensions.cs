#if AutoMapper
using AutoMapper;
#endif
#if FluentValidation
using FluentValidation;
#endif
#if MediatR
using MediatR;
using MediatR.Pipeline;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Application.DataContracts;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.InProcessMessaging.Extensions;
#if AddSamples
using NBB.Worker.Messaging;
#endif
using NBB.Messaging.Abstractions;
#if NatsMessagingTransport
using NBB.Messaging.Nats;
using System.Reflection;
using Microsoft.Extensions.Logging;
#if OpenTracing
using NBB.Messaging.OpenTracing.Subscriber;
using NBB.Messaging.OpenTracing.Publisher;
using OpenTracing;
using OpenTracing.Noop;
using Jaeger;
using Jaeger.Samplers;
using Jaeger.Reporters;
using Jaeger.Senders;
using OpenTracing.Util;
#endif
#elif KafkaMessagingTransport
using NBB.Messaging.Kafka;
#endif
#if (Resiliency)
using NBB.Resiliency;
#endif

namespace NBB.Worker
{
    public static class DependencyInjectionExtensions
    {
        public static void AddWorkerServices(this IServiceCollection services, IConfiguration configuration,
            bool useTestDoubles = false)
        {
#if MediatR
            // MediatR 
            services.AddMediatR(typeof(__AHandler__).Assembly);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            services.Scan(scan => scan.FromAssemblyOf<__AHandler__>()
                .AddClasses(classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
#endif

            // Messaging
            if (useTestDoubles)
            {
                services.AddInProcessMessaging();
            }
            else
            {
#if NatsMessagingTransport
                services.AddNatsMessaging();
#elif KafkaMessagingTransport
                services.AddKafkaMessaging();
#endif
            }

#if (Resiliency)
            services.AddResiliency();
#endif    
#if AddSamples
            services.Decorate<IMessageBusPublisher, SamplePublisherDecorator>();
#endif
#if OpenTracing
            services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();
#endif
            services.AddMessagingHost()
                .AddSubscriberServices(selector =>
                {
#if MediatR
                    selector
                        .FromMediatRHandledCommands()
                        .AddClassesAssignableTo<Command>();
#else
                    selector
                        .FromAssemblyOf<__ACommand__>()
                        .AddClassesAssignableTo<Command>();
#endif
                })
                .WithDefaultOptions()
                .UsePipeline(builder => builder
                    .UseCorrelationMiddleware()
                    .UseExceptionHandlingMiddleware()
#if OpenTracing
                    .UseMiddleware<OpenTracingMiddleware>()
#endif
#if Resiliency
                    .UseDefaultResiliencyMiddleware()
#endif
#if MediatR
                    .UseMediatRMiddleware()
#endif                
#if AddSamples
                    .UseMiddleware<SampleSubscriberPipelineMiddleware>()
#endif
                );


#if FluentValidation        
            // Validation
            services.Scan(scan => scan.FromAssemblyOf<__AValidator__>()
                .AddClasses(classes => classes.AssignableTo<IValidator>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());
#endif

#if AutoMapper
            // AutoMapper
            services.AddAutoMapper(typeof(__AMappingProfile__).Assembly);
#endif

#if OpenTracing
            // OpenTracing
            services.AddOpenTracing();

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                if (!configuration.GetValue<bool>("OpenTracing:Jeager:IsEnabled"))
                {
                    return NoopTracerFactory.Create();
                }

                string serviceName = Assembly.GetEntryAssembly().GetName().Name;

                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                ITracer tracer = new Tracer.Builder(serviceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(new ConstSampler(true))
                    .WithReporter(new RemoteReporter.Builder()
                        .WithSender(new UdpSender(
                            configuration.GetValue<string>("OpenTracing:Jeager:AgentHost"),
                            configuration.GetValue<int>("OpenTracing:Jeager:AgentPort"), 0))
                        .Build())
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });
#endif
        }
    }
}
