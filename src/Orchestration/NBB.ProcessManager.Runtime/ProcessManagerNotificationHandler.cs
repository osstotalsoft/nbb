// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Definition;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime
{
    public class ProcessManagerNotificationHandler<TDefinition, TData, TEvent> : INotificationHandler<TEvent>
        where TDefinition : IDefinition<TData>
        where TData:  IEquatable<TData>, new()
        where TEvent : INotification
    {
        private readonly ProcessExecutionCoordinator _pec;
        private readonly MessagingContextAccessor _mca;

        public ProcessManagerNotificationHandler(ProcessExecutionCoordinator pec, MessagingContextAccessor mca)
        {
            _pec = pec;
            _mca = mca;
        }

        public async Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            await _pec.Invoke<TDefinition, TData, TEvent>(notification, _mca.MessagingContext?.MessagingEnvelope?.Headers, cancellationToken);
        }
    }
}
