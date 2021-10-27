// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
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
using OpenTracing;
using OpenTracing.Noop;
using OpenTracing.Util;
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
            services.AddOpenTracingCoreServices(builder => builder
                .AddAspNetCore()
                .AddHttpHandler()
                .AddGenericDiagnostics(x => x.IgnoredListenerNames.Add("Grpc.Net.Client"))
                .AddLoggerProvider()
                .AddMicrosoftSqlClient());

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                if (!Configuration.GetValue<bool>("OpenTracing:Jaeger:IsEnabled"))
                {
                    return NoopTracerFactory.Create();
                }

                string serviceName = Assembly.GetEntryAssembly().GetName().Name;

                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                ITracer tracer = new Tracer.Builder(serviceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(new ConstSampler(true))
                    .WithReporter(new RemoteReporter.Builder()
                        .WithSender(new HttpSender(Configuration.GetValue<string>("OpenTracing:Jaeger:CollectorUrl")))
                        .Build())
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });
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
