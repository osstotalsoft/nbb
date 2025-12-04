// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Invoices.Domain.InvoiceAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Data
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly InvoicesDbContext _context;
        private readonly IUow<Invoice> _uow;

        public InvoiceRepository(InvoicesDbContext context, IUow<Invoice> uow)
        {
            _context = context;
            _uow = uow;
        }

        public async Task<Invoice> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Invoice>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            await _context.Set<Invoice>().AddAsync(invoice, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
