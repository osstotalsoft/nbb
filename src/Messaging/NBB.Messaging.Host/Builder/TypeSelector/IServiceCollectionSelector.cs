// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public interface IServiceCollectionSelector
    {
        /// <summary>
        /// Selects message types from the registrations of the given service collection instead of the one the messaging host is configured on,
        /// e.g. <c>FromServiceCollection(moduleServices).FromMediatorHandledEvents()</c> when the handlers live in a separate container (one per module).
        /// The selected types are added to the same subscriber group.
        /// </summary>
        /// <param name="services">The service collection to select message types from.</param>
        ITypeSourceSelector FromServiceCollection(IServiceCollection services);
    }
}
