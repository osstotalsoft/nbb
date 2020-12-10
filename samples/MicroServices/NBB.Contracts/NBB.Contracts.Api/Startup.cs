using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NBB.Contracts.Api.MultiTenancy;
using NBB.Contracts.ReadModel.Data;
using NBB.Correlation.AspNet;
using NBB.Messaging.MultiTenancy;
using NBB.Messaging.Nats;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.AspNet;
using NBB.MultiTenancy.Identification.Http.Extensions;

namespace NBB.Contracts.Api
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
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //services.AddKafkaMessaging();
            services.AddNatsMessaging();

            services.AddContractsReadModelDataAccess();

            services.AddMultitenancy(Configuration, _ =>
            {
                services.AddTenantRepository<TenantRepositoryMock>();
                services.AddMultiTenantMessaging();
                services.AddDefaultHttpTenantIdentification();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorrelation();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseTenantMiddleware();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
        }
    }
}
