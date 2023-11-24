// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Invoices.PublishedLanguage;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Application.CommandHandlers
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoice>
    {
        private readonly ICrudRepository<Invoice> _repository;
        public CreateInvoiceCommandHandler(ICrudRepository<Invoice> repository)
        {
            this._repository = repository;
        }

        public async Task Handle(CreateInvoice command, CancellationToken cancellationToken)
        {
            var invoice = new Invoice(command.ClientId, command.ContractId, command.Amount);
            await _repository.AddAsync(invoice, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}
