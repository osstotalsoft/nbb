// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Invoices.Domain.InvoiceAggregate;

namespace NBB.Invoices.Data
{

    public static class DependencyInjectionExtensions
    {
        public static void AddInvoicesWriteDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddScoped<IInvoiceRepository, InvoiceRepository>();

            services.AddDbContext<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });

            services.AddEventSourcingDataAccess()
                .AddEventSourcedRepository<InvoiceLock>();
        }

        public static void AddInvoicesReadDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfQuery<Invoice, InvoicesDbContext>();
            //services.AddEfReadOnlyRepository<Invoice, InvoicesDbContext>();

            services.AddDbContextPool<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });
        }

        public static void AddInvoicesDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            //services.AddEfAsyncEnumerable<Invoice, InvoicesDbContext>();
            services.AddEfQuery<Invoice, InvoicesDbContext>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();

            services.AddDbContextPool<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });
        }
    }

    


    
}
