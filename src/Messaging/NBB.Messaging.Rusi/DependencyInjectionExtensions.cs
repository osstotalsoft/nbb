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
            services.Configure<RusiOptions>(configuration.GetSection("Messaging").GetSection("Rusi"));

            services.AddGrpcClient<Rusi.RusiClient>((sp, o) =>
            {
                var opts = sp.GetRequiredService<IOptions<RusiOptions>>();
                o.Address = new Uri(opts.Value.RusiUrl);
                o.ChannelOptionsActions.Add(options =>
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
            });

            services.AddSingleton<IMessageBusSubscriber, RusiMessageBusSubscriber>();
            //services.AddSingleton<IMessageBusPublisher, RusiMessageBusPublisher>();

            return services;
        }
    }
}
