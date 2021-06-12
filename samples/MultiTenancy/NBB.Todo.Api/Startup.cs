using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.Messaging.MultiTenancy;
using NBB.Messaging.Nats;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.AspNet;
using NBB.MultiTenancy.Identification.Http.Extensions;
using NBB.Todos.Data;


namespace NBB.Todo.Api
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
            services.AddControllers();
            services.AddMessageBus().AddNatsTransport(Configuration);
            services.AddTodoDataAccess();
            
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMultitenancy(Configuration, _ =>
            {
                services
                    .AddDefaultHttpTenantIdentification()
                    .AddMultiTenantMessaging()
                    .AddTenantRepository<BasicTenantRepository>();               
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var tenancyOptions = app.ApplicationServices.GetRequiredService<IOptions<TenancyHostingOptions>>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")),
                appBuilder => appBuilder.UseTenantMiddleware());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
