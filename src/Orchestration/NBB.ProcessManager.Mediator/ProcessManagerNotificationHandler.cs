// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime;

namespace NBB.ProcessManager.Mediator
{
    /// <summary>
    /// Bridges Mediator notifications to a process manager definition. Internal and generic on purpose:
    /// the Mediator source generator skips it, it is registered in DI by AddProcessManagerMediatorHandlers().
    /// </summary>
    internal sealed class ProcessManagerNotificationHandler<TDefinition, TData, TEvent>(ProcessExecutionCoordinator pec) : INotificationHandler<TEvent>
        where TDefinition : IDefinition<TData>
        where TData : IEquatable<TData>, new()
        where TEvent : INotification
    {
        public ValueTask Handle(TEvent notification, CancellationToken cancellationToken)
            => new(pec.Invoke<TDefinition, TData, TEvent>(notification, cancellationToken));
    }
}
