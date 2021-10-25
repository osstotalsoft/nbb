// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Rusi;
using Proto.V1;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddRusiTransport(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RusiOptions>(configuration.GetSection("Messaging").GetSection("Rusi"));

            services.AddSingleton<RusiMessagingTransport>();
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<RusiMessagingTransport>());
            services.AddSingleton<IMessagingTransport>(sp => sp.GetRequiredService<RusiMessagingTransport>());

            services.AddGrpcClient<Rusi.RusiClient>((sp, o) =>
                {
                    var opts = sp.GetRequiredService<IOptions<RusiOptions>>();

                    if (string.IsNullOrEmpty(opts.Value.RusiPort))
                        throw new ArgumentNullException("RusiPort");

                    o.Address = new Uri($"http://localhost:{opts.Value.RusiPort}");
                })
                .ConfigureChannel(options =>
                {
                    options.MaxRetryAttempts = 5;
                    options.ServiceConfig = new ServiceConfig
                    {
                        MethodConfigs =
                        {
                            new MethodConfig()
                            {
                                Names = { MethodName.Default },
                                RetryPolicy = new RetryPolicy()
                                {
                                    MaxAttempts = 5,
                                    InitialBackoff = TimeSpan.FromSeconds(0.1),
                                    MaxBackoff = TimeSpan.FromSeconds(1),
                                    BackoffMultiplier = 1.5,
                                    RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.Aborted }
                                }
                            }
                        }
                    };
                });


            services.PostConfigureAll<RusiOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.RusiPort))
                    options.RusiPort = Environment.GetEnvironmentVariable("RUSI_GRPC_PORT");

                if (string.IsNullOrEmpty(options.RusiPort))
                    throw new ArgumentNullException("Rusi.RusiPort");
            });

            return services;
        }
    }
}
