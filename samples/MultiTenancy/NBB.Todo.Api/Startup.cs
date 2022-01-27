// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.AspNet;
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
            services.AddSwaggerGen(options => options.OperationFilter<SwaggerTenantHeaderFilter>());

            services.AddMultitenancy(Configuration)
                .AddDefaultHttpTenantIdentification()
                .AddMultiTenantMessaging()
                .AddTenantRepository<ConfigurationTenantRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")),
                appBuilder => appBuilder.UseTenantMiddleware());

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        }
    }
}
