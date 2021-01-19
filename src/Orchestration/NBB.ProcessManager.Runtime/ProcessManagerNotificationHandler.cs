﻿using MediatR;
using NBB.ProcessManager.Definition;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime
{
    public class ProcessManagerNotificationHandler<TDefinition, TData, TEvent> : INotificationHandler<TEvent>
        where TDefinition : IDefinition<TData>
        where TData: new()
        where TEvent : INotification
    {
        private readonly ProcessExecutionCoordinator _pec;

        public ProcessManagerNotificationHandler(ProcessExecutionCoordinator pec)
        {
            _pec = pec;
        }

        public async Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            await _pec.Invoke<TDefinition, TData, TEvent>(notification, cancellationToken);
        }
    }
}