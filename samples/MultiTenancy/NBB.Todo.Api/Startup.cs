// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NBB.Correlation;
using NBB.Correlation.AspNet;
using NBB.Messaging.OpenTelemetry;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.AspNet;
using NBB.Todos.Data;
using NBB.Tools.Serilog.Enrichers.TenantId;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Reflection;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

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
            services.AddSwaggerGen(options =>
            {
                if (Configuration.IsMultiTenant())
                {
                    options.OperationFilter<SwaggerTenantHeaderFilter>();
                }
            });

            services.AddMultitenancy(Configuration)
                .AddDefaultHttpTenantIdentification()
                .AddMultiTenantMessaging()
                .AddTenantRepository<ConfigurationTenantRepository>();
            services.AddSingleton<TenantEnricher>();

            services.AddProblemDetails(config => ConfigureProblemDetails(config));

            var assembly = Assembly.GetExecutingAssembly().GetName();
            void configureResource(ResourceBuilder r) =>
                r.AddService(assembly.Name, serviceVersion: assembly.Version?.ToString(), serviceInstanceId: Environment.MachineName);

            if (Configuration.GetValue<bool>("OpenTelemetry:TracingEnabled"))
            {
                Sdk.SetDefaultTextMapPropagator(new JaegerPropagator());

                services.AddOpenTelemetry().WithTracing(builder => builder
                        .ConfigureResource(configureResource)
                        .SetSampler(new AlwaysOnSampler())
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddMessageBusInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
                        .AddOtlpExporter()
                );
                services.Configure<OtlpExporterOptions>(Configuration.GetSection("OpenTelemetry:Otlp"));
            }

            if (Configuration.GetValue<bool>("OpenTelemetry:MetricsEnabled"))
            {
                services.AddOpenTelemetry().WithMetrics(options =>
                {
                    options.ConfigureResource(configureResource)
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddPrometheusExporter();
                });
            }

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

            app.UseCorrelation();
            app.UseWhen(
                ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")),
                appBuilder => appBuilder.UseTenantMiddleware());

            app.UseProblemDetails();


            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        }

        //TODO refactor ProblemDetails with AspNet.Core implementations 
        public static ProblemDetailsOptions ConfigureProblemDetails(ProblemDetailsOptions options, bool includeExceptionDetails = true)
        {
            includeExceptionDetails = includeExceptionDetails && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            options.IncludeExceptionDetails = (_context, _exception) => includeExceptionDetails;
            options.MapStatusCode = context => new StatusCodeProblemDetails(context.Response.StatusCode);

            options.Map<NotImplementedException>(_ex => new StatusCodeProblemDetails(StatusCodes.Status501NotImplemented));

            options.Map<Exception>(ex =>
            {
                var det = new ProblemDetailsException
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unexpected error",
                    CorrelationId = CorrelationManager.GetCorrelationId()?.ToString(),
                    Detail = includeExceptionDetails ? ex.Message : "Unexpected error",
                    Type = ex.GetType().FullName
                };
                return det;
            });

            return options;
        }
    }

    public class ProblemDetailsException : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public ProblemDetailsException()
        {
        }

        public string CorrelationId { get; set; }
    }
}
