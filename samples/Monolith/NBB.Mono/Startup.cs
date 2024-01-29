// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Application.MediatR;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Core.Abstractions;
using NBB.Correlation.AspNet;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.Invoices.Application.CommandHandlers;
using NBB.Invoices.Data;
using NBB.Payments.Application.CommandHandlers;
using NBB.Payments.Data;
using NBB.Domain;
using NBB.Messaging.Host;
using Microsoft.Extensions.Hosting;
using System.Linq;
using MicroServicesOrchestration;
using NBB.Core.DependencyInjection;

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
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                typeof(ContractCommandHandlers).Assembly,
                typeof(CreateInvoiceCommandHandler).Assembly,
                typeof(PayPayableCommandHandler).Assembly));

            services.AddMessageBus().AddInProcessTransport();

            services.AddContractsWriteModelDataAccess();
            services.AddContractsReadModelDataAccess();
            services.AddInvoicesDataAccess();
            services.AddPaymentsDataAccess();

            services.AddEventStore(es =>
            {
                es.UseNewtownsoftJson(new SingleValueObjectConverter());
                es.UseAdoNetEventRepository(opts => opts.FromConfiguration());
            });

            var integrationMessageAssemblies = new[] {
                typeof(NBB.Contracts.PublishedLanguage.ContractValidated).Assembly,
                typeof(NBB.Invoices.PublishedLanguage.InvoiceCreated).Assembly,
                typeof(NBB.Payments.PublishedLanguage.PayableCreated).Assembly,
            };

            services.AddMessagingHost(
                Configuration,
                hostBuilder => hostBuilder
                .Configure(configBuilder => configBuilder
                    .AddSubscriberServices(subscriberBuiler => subscriberBuiler
                        .FromMediatRHandledCommands().AddClassesWhere(t => integrationMessageAssemblies.Contains(t.Assembly))
                        .FromMediatRHandledEvents().AddClassesWhere(t => integrationMessageAssemblies.Contains(t.Assembly))
                    )
                    .WithDefaultOptions()
                    .UsePipeline(pipelineBuilder => pipelineBuilder
                        .UseExceptionHandlingMiddleware()
                        .UseCorrelationMiddleware()
                        .UseDefaultResiliencyMiddleware()
                        .UseMediatRMiddleware()
                    )
                )
            );

            services.AddProcessManager(typeof(InvoicingProcessManager).Assembly);

            services.DecorateOpenGenericWhen(typeof(IUow<>), typeof(DomainUowDecorator<>),
                serviceType => typeof(IEventedAggregateRoot).IsAssignableFrom(serviceType.GetGenericArguments()[0]));
            services.DecorateOpenGenericWhen(typeof(IUow<>), typeof(MediatorUowDecorator<>),
                serviceType => typeof(IEventedEntity).IsAssignableFrom(serviceType.GetGenericArguments()[0]));
            services.DecorateOpenGenericWhen(typeof(IUow<>), typeof(EventStoreUowDecorator<>),
                serviceType => typeof(IEventedEntity).IsAssignableFrom(serviceType.GetGenericArguments()[0]) &&
                               typeof(IIdentifiedEntity).IsAssignableFrom(serviceType.GetGenericArguments()[0]));
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
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
