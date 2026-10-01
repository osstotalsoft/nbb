// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.Mediator;
using NBB.Core.Abstractions;
using NBB.Core.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediatorApplicationDependencyInjectionExtensions
    {
        private const string SingleMediatorLibrary =
            "An application uses a single mediator library: call either AddMediatorIntegration() or AddMediatRIntegration().";

        /// <summary>
        /// Registers the Mediator implementations of the NBB application ports:
        /// the <see cref="IEventPublisher"/> (<see cref="MediatorEventPublisher"/>) and the <see cref="IContractKindClassifier"/> (<see cref="MediatorContractKindClassifier"/>).
        /// Mediator itself is registered by the generated <c>AddMediator()</c>, in the project that references Mediator.SourceGenerator.
        /// </summary>
        public static IServiceCollection AddMediatorIntegration(this IServiceCollection services)
            => services
                .AddMediatorEventPublisher()
                .AddMediatorContractClassification();

        /// <summary>
        /// Registers <see cref="MediatorEventPublisher"/> as the <see cref="IEventPublisher"/>, used by event sourced repositories
        /// and <c>EventPublishingUowDecorator</c> to publish events to Mediator notification handlers.
        /// </summary>
        private static IServiceCollection AddMediatorEventPublisher(this IServiceCollection services)
            => services.AddSingleImplementation<IEventPublisher, MediatorEventPublisher>(ServiceLifetime.Scoped, SingleMediatorLibrary);

        /// <summary>
        /// Registers <see cref="MediatorContractKindClassifier"/> as the <see cref="IContractKindClassifier"/>,
        /// used by NBB.Messaging.MultiTenancy and NBB.Messaging.BackwardCompatibility.
        /// </summary>
        private static IServiceCollection AddMediatorContractClassification(this IServiceCollection services)
            => services.AddSingleImplementation<IContractKindClassifier, MediatorContractKindClassifier>(ServiceLifetime.Singleton, SingleMediatorLibrary);
    }
}
