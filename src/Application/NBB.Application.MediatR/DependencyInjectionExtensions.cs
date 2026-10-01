// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.MediatR;
using NBB.Core.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediatRApplicationDependencyInjectionExtensions
    {
        /// <summary>
        /// Registers the MediatR implementations of the NBB application ports:
        /// the <see cref="IEventPublisher"/> (<see cref="MediatREventPublisher"/>) and the <see cref="IContractKindClassifier"/> (<see cref="MediatRContractKindClassifier"/>).
        /// An application uses a single mediator library: do not call AddMediatorIntegration() too.
        /// </summary>
        public static IServiceCollection AddMediatRIntegration(this IServiceCollection services)
            => services
                .AddScoped<IEventPublisher, MediatREventPublisher>()
                .AddSingleton<IContractKindClassifier, MediatRContractKindClassifier>();
    }
}
