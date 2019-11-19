using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Domain.InvoiceAggregate
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
