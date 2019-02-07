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
        }
    }
}
