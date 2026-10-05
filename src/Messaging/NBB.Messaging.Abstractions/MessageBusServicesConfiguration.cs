// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace NBB.Messaging.Abstractions
{
    /// <summary>
    /// A configuration of the message bus services registered before the message bus itself,
    /// applied by <see cref="DependencyInjectionExtensions.AddMessageBus"/>.
    /// See <see cref="DependencyInjectionExtensions.ConfigureMessageBusServices"/>.
    /// </summary>
    internal sealed class MessageBusServicesConfiguration(Action<IServiceCollection> configure)
    {
        internal Action<IServiceCollection> Configure { get; } = configure;
    }
}
