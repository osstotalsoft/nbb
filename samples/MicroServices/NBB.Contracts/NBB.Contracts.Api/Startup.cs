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
using NBB.Messaging.OpenTracing;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

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
                    .AddMessageBus()
                    .AddNatsTransport(Configuration)
                    .UseTopicResolutionBackwardCompatibility(Configuration);
            }
            else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
            {

                services
                    .AddMessageBus()
                    .AddRusiTransport(Configuration)
                    .UseTopicResolutionBackwardCompatibility(Configuration);
            }
            else
            {
                throw new Exception($"Messaging:Transport={transport} not supported");
            }

            services.AddContractsReadModelDataAccess();

            //services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();

            // OpenTracing
            //services.AddOpenTracingCoreServices(builder => builder
            //    .AddAspNetCore()
            //    .AddHttpHandler()
            //    .AddGenericDiagnostics(x => x.IgnoredListenerNames.Add("Grpc.Net.Client"))
            //    .AddLoggerProvider()
            //    .AddMicrosoftSqlClient());


            string serviceName = Configuration.GetValue<string>("OpenTelemetry:Jaeger:ServiceName");
            Action<ResourceBuilder> configureResource =
                r => r.AddService(serviceName, serviceVersion: "1.0", serviceInstanceId: Environment.MachineName);

            if (Configuration.GetValue<bool>("OpenTelemetry:TracingEnabled"))
            {
                Sdk.SetDefaultTextMapPropagator(new JaegerPropagator());

                services.AddOpenTelemetryTracing(builder => builder
                        .ConfigureResource(configureResource)
                        .AddSource(MessagingTags.ComponentMessaging)
                        .SetSampler(new AlwaysOnSampler())
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                        //.AddAspNetInstrumentation(o => o.Enrich = (activity, eventName, rawObject) =>
                        //{
                        //    if (eventName.Equals("OnStartActivity"))
                        //    {
                        //        if (rawObject is HttpRequest httpRequest)
                        //        {
                        //            activity.SetTag("sampling.priority", "1");
                        //        }
                        //    }
                        //})
                        .AddEntityFrameworkCoreInstrumentation()
                        //.AddJaegerExporter(options =>
                        //{
                        //    options.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
                        //    options.Endpoint = new Uri("http://kube-worker1.totalsoft.local:31034");//api/traces");
                        //})
                        .AddJaegerExporter()
                    //.AddConsoleExporter()
                );
                services.Configure<JaegerExporterOptions>(Configuration.GetSection("OpenTelemetry:Jaeger"));

                // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                //services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));

                // For options which can be bound from IConfiguration.
                //services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));
            }

            if (Configuration.GetValue<bool>("OpenTelemetry:MetricsEnabled"))
            {
                services.AddOpenTelemetryMetrics(options =>
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
