using System;
using System.Threading.Tasks;

namespace NBB.Invoices.Domain.InvoiceAggregate
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetByIdAsync(Guid id);
        Task AddAsync(Invoice invoice);
        Task SaveChangesAsync();
    }
}
