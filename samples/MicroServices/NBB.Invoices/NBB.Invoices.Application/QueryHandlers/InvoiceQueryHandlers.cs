using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Data.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using NBB.Invoices.PublishedLanguage.IntegrationQueries;

namespace NBB.Invoices.Application.QueryHandlers
{
    public class InvoiceQueryHandlers : IRequestHandler<GetInvoice.Query, GetInvoice.Model>
    {
        private readonly ICrudRepository<Invoice> _repository;

        public InvoiceQueryHandlers(ICrudRepository<Invoice> repository)
        {
            _repository = repository;
        }

        public async Task<GetInvoice.Model> Handle(GetInvoice.Query request, CancellationToken cancellationToken)
        {
            var x = await _repository.GetByIdAsync(request.InvoiceId);
            var result = new GetInvoice.Model {InvoiceId = x.InvoiceId, ContractId = x.ContractId};
            return result;
        }
    }
}
