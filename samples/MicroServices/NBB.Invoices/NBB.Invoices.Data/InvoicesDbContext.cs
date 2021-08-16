// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using NBB.Invoices.Domain.InvoiceAggregate;

namespace NBB.Invoices.Data
{
    public class InvoicesDbContext : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }


        public InvoicesDbContext(DbContextOptions<InvoicesDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Invoice>(builder =>
            {
                builder.ToTable("Invoices")
                    .HasKey(c => c.InvoiceId);

                builder
                    .Property(c => c.InvoiceId)
                    .ValueGeneratedNever();
            });
        }
    }
}