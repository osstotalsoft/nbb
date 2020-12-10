using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Application.DataContracts;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Core.Abstractions;
using NBB.Correlation.AspNet;
using NBB.Domain.Abstractions;
using NBB.EventStore;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet;
using NBB.Invoices.Application.CommandHandlers;
using NBB.Invoices.Data;
using NBB.Messaging.InProcessMessaging.Extensions;
using NBB.Payments.Application.CommandHandlers;
using NBB.Payments.Data;
using NBB.Core.DependencyInjection;
using NBB.Domain;
using NBB.Messaging.Host;
using NBB.Resiliency;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Host.Builder;

namespace NBB.Mono
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddMediatR(
                typeof(ContractCommandHandlers).Assembly,
                typeof(InvoiceCommandHandlers).Assembly,
                typeof(PayableCommandHandlers).Assembly);

            services.AddInProcessMessaging();

            services.AddContractsWriteModelDataAccess();
            services.AddContractsReadModelDataAccess();
            services.AddInvoicesDataAccess();
            services.AddInvoicesWriteDataAccess();
            services.AddPaymentsDataAccess();
            services.AddPaymentsWriteDataAccess();

            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer(new[] { new SingleValueObjectConverter() })
                .WithAdoNetEventRepository();

            services.AddResiliency();

            services.AddMessagingHost()
                .AddSubscriberServices(config => config
                    .FromMediatRHandledCommands().AddClassesAssignableTo<Command>()
                    .FromMediatRHandledEvents().AddClassesAssignableTo<Event>()
                )
                .WithDefaultOptions()
                .UsePipeline(pipelineBuilder => pipelineBuilder
                    .UseExceptionHandlingMiddleware()
                    .UseCorrelationMiddleware()
                    .UseDefaultResiliencyMiddleware()
                    .UseMediatRMiddleware()
                );

            services.DecorateOpenGenericWhen(typeof(IUow<>), typeof(DomainUowDecorator<>),
                serviceType => typeof(IEventedAggregateRoot).IsAssignableFrom(serviceType.GetGenericArguments()[0]));
            services.DecorateOpenGenericWhen(typeof(IUow<>), typeof(MediatorUowDecorator<>),
                serviceType => typeof(IEventedEntity).IsAssignableFrom(serviceType.GetGenericArguments()[0]));
            services.DecorateOpenGenericWhen(typeof(IUow<>), typeof(EventStoreUowDecorator<>),
                serviceType => typeof(IEventedEntity).IsAssignableFrom(serviceType.GetGenericArguments()[0]) && typeof(IIdentifiedEntity).IsAssignableFrom(serviceType.GetGenericArguments()[0]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorrelation();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
        }
    }
}
