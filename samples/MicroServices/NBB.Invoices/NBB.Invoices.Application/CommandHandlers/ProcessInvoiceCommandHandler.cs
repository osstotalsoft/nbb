// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Invoices.PublishedLanguage;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Application.CommandHandlers
{
    public class ProcessInvoiceCommandHandler : IRequestHandler<ProcessInvoice>
    {
        private readonly ICrudRepository<Invoice> _invocieRepository;
        private readonly IEventSourcedRepository<InvoiceLock> _invoiceLockRepository;

        public ProcessInvoiceCommandHandler(ICrudRepository<Invoice> invocieRepository, IEventSourcedRepository<InvoiceLock> invoiceLockRepository)
        {
            this._invocieRepository = invocieRepository;
            this._invoiceLockRepository = invoiceLockRepository;
        }

        public async Task<Unit> Handle(ProcessInvoice command, CancellationToken cancellationToken)
        {
            var invoiceLock = await _invoiceLockRepository.GetByIdAsync(command.InvoiceId) ??
                new InvoiceLock(command.InvoiceId, lockTimeoutMs: 10000);

            invoiceLock.Acquire(); //throws InvoiceLockAlreadyAcquiredException if locked

            await _invoiceLockRepository.SaveAsync(invoiceLock); //throws ConcurencyException => retry forever
            //aici sunt protejat
            try
            {
                await InternalHandle(command, cancellationToken);
            }
            finally
            {
                invoiceLock.Release();
                await _invoiceLockRepository.SaveAsync(invoiceLock);
            }

            return Unit.Value;
        }

        private async Task<Unit> InternalHandle(ProcessInvoice command, CancellationToken cancellationToken)
        {
            var invoice = await _invocieRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
            invoice.Process(); //takes 1000 ms cpu time
            await _invocieRepository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    
}
