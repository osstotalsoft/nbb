// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Http;
using Proto.V1;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddHttpTransport(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<HttpOptions>(configuration.GetSection("Messaging").GetSection("Http"));

            services.AddSingleton<IMessagingTransport, HttpMessagingTransport>();

            services.AddGrpcClient<Rusi.RusiClient>((sp, o) =>
                {
                    var opts = sp.GetRequiredService<IOptions<HttpOptions>>();

                    if (string.IsNullOrEmpty(opts.Value.RusiPort))
                        throw new ArgumentNullException("RusiPort");

                    o.Address = new Uri($"http://localhost:{opts.Value.RusiPort}");
                })
                .ConfigureChannel(options =>
                {
                    options.MaxRetryAttempts = 200;
                    options.ServiceConfig = new ServiceConfig
                    {
                        MethodConfigs =
                        {
                            new MethodConfig()
                            {
                                Names = { MethodName.Default },
                                RetryPolicy = new RetryPolicy()
                                {
                                    MaxAttempts = 200,
                                    InitialBackoff = TimeSpan.FromSeconds(10),
                                    MaxBackoff = TimeSpan.FromMinutes(30),
                                    BackoffMultiplier = 1.5,
                                    RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.Aborted }
                                }
                            }
                        }
                    };
                });


            services.PostConfigureAll<HttpOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.RusiPort))
                    options.RusiPort = Environment.GetEnvironmentVariable("RUSI_GRPC_PORT");

                if (string.IsNullOrEmpty(options.PubsubName))
                    throw new ArgumentNullException("Rusi.PubsubName");

                if (string.IsNullOrEmpty(options.RusiPort))
                    throw new ArgumentNullException("Rusi.RusiPort");
            });

            return services;
        }
    }
}
