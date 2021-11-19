// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Grpc.Core;
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
            services.AddOptions<RusiOptions>()
                .Bind(configuration.GetSection("Messaging").GetSection("Rusi"))
                .Validate(options => !string.IsNullOrEmpty(options.RusiPort),
                    "missing RusiPort, try add RUSI_GRPC_PORT environment variable");

            services.AddSingleton<RusiMessagingTransport>();
            services.AddSingleton<ITransportMonitor>(sp => sp.GetRequiredService<RusiMessagingTransport>());
            services.AddSingleton<IMessagingTransport>(sp => sp.GetRequiredService<RusiMessagingTransport>());

            services.AddGrpcClient<Rusi.RusiClient>((sp, o) =>
                {
                    var opts = sp.GetRequiredService<IOptions<RusiOptions>>();
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
                                    RetryableStatusCodes = { StatusCode.Unavailable,
                                        StatusCode.Unknown,
                                        StatusCode.DeadlineExceeded,
                                        StatusCode.Aborted }
                                }
                            }
                        }
                    };
                });


            services.PostConfigureAll<RusiOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.RusiPort))
                    options.RusiPort = Environment.GetEnvironmentVariable("RUSI_GRPC_PORT");
            });

            return services;
        }
    }
}
