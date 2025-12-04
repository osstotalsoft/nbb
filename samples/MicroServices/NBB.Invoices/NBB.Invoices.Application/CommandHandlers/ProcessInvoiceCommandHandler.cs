// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Invoices.PublishedLanguage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Application.CommandHandlers
{
    public class ProcessInvoiceCommandHandler : IRequestHandler<ProcessInvoice>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IEventSourcedRepository<InvoiceLock> _invoiceLockRepository;

        public ProcessInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IEventSourcedRepository<InvoiceLock> invoiceLockRepository)
        {
            this._invoiceRepository = invoiceRepository;
            this._invoiceLockRepository = invoiceLockRepository;
        }

        public Task Handle(ProcessInvoice command, CancellationToken cancellationToken)
            => UsingInvoiceLock(command.InvoiceId, lockTimeoutMs: 10000, async () =>
            {
                var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
                invoice.Process(); //takes 1000 ms cpu time
                await _invoiceRepository.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            });

        private async Task<T> UsingInvoiceLock<T>(Guid invoiceId, int lockTimeoutMs, Func<Task<T>> func)
        {
            var invoiceLock = await _invoiceLockRepository.GetByIdAsync(invoiceId) ??
                new InvoiceLock(invoiceId, lockTimeoutMs);

            invoiceLock.Acquire(); //throws InvoiceLockAlreadyAcquiredException if locked
            await _invoiceLockRepository.SaveAsync(invoiceLock); //throws ConcurencyException => retry forever

            try
            {
                return await func();
            }
            finally
            {
                invoiceLock.Release();
                await _invoiceLockRepository.SaveAsync(invoiceLock);
            }
        }
    }
}
