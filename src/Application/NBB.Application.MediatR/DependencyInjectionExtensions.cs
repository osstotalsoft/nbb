// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.MediatR;
using NBB.Core.Abstractions;
using NBB.Core.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediatRApplicationDependencyInjectionExtensions
    {
        private const string SingleMediatorLibrary =
            "An application uses a single mediator library: call either AddMediatorIntegration() or AddMediatRIntegration().";

        /// <summary>
        /// Registers the MediatR implementations of the NBB application ports:
        /// the <see cref="IEventPublisher"/> (<see cref="MediatREventPublisher"/>) and the <see cref="IContractKindClassifier"/> (<see cref="MediatRContractKindClassifier"/>).
        /// </summary>
        public static IServiceCollection AddMediatRIntegration(this IServiceCollection services)
            => services
                .AddMediatREventPublisher()
                .AddMediatRContractClassification();

        /// <summary>
        /// Registers <see cref="MediatREventPublisher"/> as the <see cref="IEventPublisher"/>, used by event sourced repositories
        /// and <c>EventPublishingUowDecorator</c> to publish events to MediatR notification handlers.
        /// </summary>
        private static IServiceCollection AddMediatREventPublisher(this IServiceCollection services)
            => services.AddSingleImplementation<IEventPublisher, MediatREventPublisher>(ServiceLifetime.Scoped, SingleMediatorLibrary);

        /// <summary>
        /// Registers <see cref="MediatRContractKindClassifier"/> as the <see cref="IContractKindClassifier"/>,
        /// used by NBB.Messaging.MultiTenancy and NBB.Messaging.BackwardCompatibility.
        /// </summary>
        private static IServiceCollection AddMediatRContractClassification(this IServiceCollection services)
            => services.AddSingleImplementation<IContractKindClassifier, MediatRContractKindClassifier>(ServiceLifetime.Singleton, SingleMediatorLibrary);
    }
}
