using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Todo.Worker.Application;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats;
using NBB.Todos.Data;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Identification.Messaging.Extensions;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.MultiTenancy.Abstractions.Options;
using Microsoft.Extensions.Options;

namespace NBB.Todo.Worker
{
    public static class DependencyInjectionExtensions
    {
        public static void AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MediatR 
            services.AddMediatR(typeof(CreateTodoTaskHandler).Assembly);

            // Data
            services.AddTodoDataAccess();

            // Messaging
            services.AddMessageBus().AddNatsTransport(configuration);

            services.AddMessagingHost(hostBuilder => hostBuilder
                .Configure(configBuilder =>
                {
                    var tenancyOptions = configBuilder.ApplicationServices.GetRequiredService<IOptions<TenancyHostingOptions>>();
                    var isMultiTenant = tenancyOptions?.Value?.TenancyType != TenancyType.None;

                    configBuilder
                        .AddSubscriberServices(selector => selector.
                            FromMediatRHandledCommands().AddAllClasses())
                        .WithDefaultOptions()
                        .UsePipeline(builder => builder
                            .UseCorrelationMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .When(isMultiTenant, x => x.UseTenantMiddleware())
                            .UseDefaultResiliencyMiddleware()
                            .UseMediatRMiddleware()
                    );
                }));

            // Multitenancy
            services.AddMultitenancy(configuration, _ =>
            {
                services.AddMultiTenantMessaging()
                        .AddDefaultMessagingTenantIdentification()
                        .AddTenantRepository<BasicTenantRepository>();

                 services.AddMultiTenantTodoDataAccess();
            });
        }
    }
}
