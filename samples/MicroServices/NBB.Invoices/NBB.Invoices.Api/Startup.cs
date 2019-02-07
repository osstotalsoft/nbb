using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.AspNet;
using NBB.Invoices.Data;
using NBB.Invoices.PublishedLanguage.IntegrationQueries;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Kafka;
using NBB.Resiliency;

namespace NBB.Invoices.Api
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
            services.AddKafkaMessaging();

            services.AddInvoicesReadDataAccess();
            services.AddMessagingHost()
                .UsePipeline(config => config
                    .UseCorrelationMiddleware()
                    .UseExceptionHandlingMiddleware())
                .AddSubscriberServices(config => config
                    .FromMediatRHandledQueries().AddAllClasses())
                .WithDefaultOptions();

                
            services.AddMessageBusMediator();

            services.AddResiliency();
            services.AddSingleton<IHostedService, MessageBusSubscriberService<MessagingResponse<GetInvoice.Model>>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            app.UseCorrelation();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
