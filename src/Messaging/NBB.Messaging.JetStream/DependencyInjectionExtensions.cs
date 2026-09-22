// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NATS.Extensions.Microsoft.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.JetStream;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddJetStreamTransport(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JetStreamOptions>(configuration.GetSection("Messaging").GetSection("JetStream"));

            services.AddNatsClient(nats =>
            {
                //nats.WithPoolSize(10);
                nats.ConfigureOptions(opts =>
                    opts.Configure<IOptions<JetStreamOptions>>((builder, jetStreamOptions) =>
                        builder.Opts = builder.Opts with
                        {
                            Url = jetStreamOptions.Value.NatsUrl,
                            ConnectTimeout = TimeSpan.FromMilliseconds(jetStreamOptions.Value.ConnectTimeout ?? 20000), // initial connection
                            CommandTimeout = TimeSpan.FromMilliseconds(jetStreamOptions.Value.CommandTimeout ?? 20000), // core publish/flush into the write channel
                            RequestTimeout = TimeSpan.FromMilliseconds(jetStreamOptions.Value.RequestTimeout ?? 20000), // request-reply, incl. JetStream PubAck
                        }));
            });

            services.AddSingleton<JetStreamMessagingTransport>();
            services.AddSingleton<IMessagingTransport>(sp => sp.GetRequiredService<JetStreamMessagingTransport>());
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<JetStreamMessagingTransport>());


            return services;
        }
    }
}
