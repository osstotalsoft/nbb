// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.Mediator;
using NBB.Core.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediatorApplicationDependencyInjectionExtensions
    {
        /// <summary>
        /// Registers the Mediator implementations of the NBB application ports:
        /// the <see cref="IEventPublisher"/> (<see cref="MediatorEventPublisher"/>) and the <see cref="IContractKindClassifier"/> (<see cref="MediatorContractKindClassifier"/>).
        /// Mediator itself is registered by the generated <c>AddMediator()</c>, in the project that references Mediator.SourceGenerator.
        /// An application uses a single mediator library: do not call AddMediatRIntegration() too.
        /// </summary>
        public static IServiceCollection AddMediatorIntegration(this IServiceCollection services)
            => services
                .AddScoped<IEventPublisher, MediatorEventPublisher>()
                .AddSingleton<IContractKindClassifier, MediatorContractKindClassifier>();
    }
}
