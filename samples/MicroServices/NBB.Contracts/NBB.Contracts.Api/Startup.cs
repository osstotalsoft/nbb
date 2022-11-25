// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NBB.Contracts.ReadModel.Data;
using NBB.Correlation.AspNet;
using NBB.Messaging.Abstractions;
using NBB.Messaging.OpenTracing.Publisher;
using System;
using System.Reflection;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Exporter;
using NBB.Messaging.OpenTracing;

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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contracts API", Version = "v1" });
            });

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

            services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();

            // OpenTracing
            //services.AddOpenTracingCoreServices(builder => builder
            //    .AddAspNetCore()
            //    .AddHttpHandler()
            //    .AddGenericDiagnostics(x => x.IgnoredListenerNames.Add("Grpc.Net.Client"))
            //    .AddLoggerProvider()
            //    .AddMicrosoftSqlClient());

            if (Configuration.GetValue<bool>("OpenTracing:Jaeger:IsEnabled"))
            {

                var currentAssemblyInfo = Assembly.GetEntryAssembly().GetName();

                Sdk.SetDefaultTextMapPropagator(new JaegerPropagator());

                services.AddOpenTelemetryTracing(builder => builder
                    .ConfigureResource(r => r.AddService(serviceName: currentAssemblyInfo.Name, serviceVersion: currentAssemblyInfo.Version.ToString()))
                    //.AddSource(Assembly.GetEntryAssembly().GetName().Name)
                    .AddSource(MessagingTags.ComponentMessaging)
                    .SetSampler(new AlwaysOnSampler())                  
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
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
            }


            //services.AddSingleton<ITracer>(serviceProvider =>
            //{
            //    if (!Configuration.GetValue<bool>("OpenTracing:Jaeger:IsEnabled"))
            //    {
            //        return NoopTracerFactory.Create();
            //    }

            //    string serviceName = Assembly.GetEntryAssembly().GetName().Name;

            //    ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            //    ITracer tracer = new Tracer.Builder(serviceName)
            //        .WithLoggerFactory(loggerFactory)
            //        .WithSampler(new ConstSampler(true))
            //        .WithReporter(new RemoteReporter.Builder()
            //            .WithSender(new HttpSender(Configuration.GetValue<string>("OpenTracing:Jaeger:CollectorUrl")))
            //            .Build())
            //        .Build();

            //    GlobalTracer.Register(tracer);

            //    return tracer;
            //});
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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
        }
    }
}
