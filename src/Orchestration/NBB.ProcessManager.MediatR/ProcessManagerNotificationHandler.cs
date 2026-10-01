// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.ProcessManager.Definition;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime
{
    public class ProcessManagerNotificationHandler<TDefinition, TData, TEvent>(ProcessExecutionCoordinator pec) : INotificationHandler<TEvent>
        where TDefinition : IDefinition<TData>
        where TData:  IEquatable<TData>, new()
        where TEvent : INotification
    {
        public Task Handle(TEvent notification, CancellationToken cancellationToken)
            => pec.Invoke<TDefinition, TData, TEvent>(notification, cancellationToken);
    }
}
