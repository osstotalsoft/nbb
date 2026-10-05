// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.AspNet;
using NBB.Invoices.Data;

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
            services.AddSingleton(Configuration);
            //services.AddKafkaMessaging();
            //Default transport is NATS. To opt in to Kafka, replace the line below with: services.AddMessageBus().AddKafkaTransport(Configuration);
            services.AddMessageBus().AddNatsTransport(Configuration);
            services.AddInvoicesReadDataAccess();
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
