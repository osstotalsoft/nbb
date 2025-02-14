// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NBB.Contracts.ReadModel.Data;
using NBB.Correlation.AspNet;
using NBB.Messaging.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Reflection;

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
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contracts API", Version = "v1" }); });

            services.AddSingleton(Configuration);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var transport = Configuration.GetValue("Messaging:Transport", "NATS");
            if (transport.Equals("NATS", StringComparison.InvariantCultureIgnoreCase))
            {
                services
                    .AddMessageBus(Configuration)
                    .AddNatsTransport(Configuration)
                    .UseTopicResolutionBackwardCompatibility(Configuration);
            }
            else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
            {

                services
                    .AddMessageBus(Configuration)
                    .AddRusiTransport(Configuration)
                    .UseTopicResolutionBackwardCompatibility(Configuration);
            }
            else
            {
                throw new Exception($"Messaging:Transport={transport} not supported");
            }

            services.AddContractsReadModelDataAccess();

            var assembly = Assembly.GetExecutingAssembly().GetName();
            void configureResource(ResourceBuilder r) =>
                r.AddService(assembly.Name, serviceVersion: assembly.Version?.ToString(), serviceInstanceId: Environment.MachineName);

            if (Configuration.GetValue<bool>("OpenTelemetry:TracingEnabled"))
            {
                Sdk.SetDefaultTextMapPropagator(new JaegerPropagator());

                services.AddOpenTelemetry().WithTracing(builder => builder
                        .ConfigureResource(configureResource)
                        .SetSampler(new AlwaysOnSampler())
                        .AddMessageBusInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
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
            app.UseCorrelation();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contracts API v1"));
            }

            app.UseRouting();

            if (Configuration.GetValue<bool>("OpenTelemetry:MetricsEnabled"))
                app.UseOpenTelemetryPrometheusScrapingEndpoint();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
