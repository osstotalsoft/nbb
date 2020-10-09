﻿#if AspNetApp
#if HealthCheck 
using NBB.Worker.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Worker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWorkerServices(Configuration);

#if HealthCheck
            services.AddHealthChecks() // Registers health checks services
                // Add a health check for a SQL database
                .AddCheck("SQL database",
                    new SqlConnectionHealthCheck("Log_Database", Configuration["ConnectionStrings:Log_Database"]))
                .AddCheck<GCInfoHealthCheck>("GC");
            
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if HealthCheck
            app.UseHealthChecks("/health", options: new HealthCheckOptions()
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
#endif
        }
    }
}
#endif