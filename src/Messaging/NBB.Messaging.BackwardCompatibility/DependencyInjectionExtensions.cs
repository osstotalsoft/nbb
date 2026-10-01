// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;
using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.BackwardCompatibility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection UseTopicResolutionBackwardCompatibility(this IServiceCollection services, IConfiguration configuration)
        {
            var topicResolutionCompatibility = configuration.GetSection("Messaging")?["TopicResolutionCompatibility"];
            if (string.IsNullOrWhiteSpace(topicResolutionCompatibility) || topicResolutionCompatibility == "NBB_4")
            {
                services.Decorate<ITopicRegistry, NBB4TopicRegistryDecorator>();
                services.AddOptions<NBB4TopicResolutionOptions>()
                    .Validate<IServiceProvider>(
                        (_, sp) => sp.GetService<IContractKindClassifier>() != null,
                        "NBB 4 topic resolution requires an IContractKindClassifier: call AddMediatorIntegration() (NBB.Application.Mediator) or AddMediatRIntegration() (NBB.Application.MediatR).")
                    .ValidateOnStart();
            }
            return services;
        }

        private sealed class NBB4TopicResolutionOptions
        {
        }
    }
}
