using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Data.EntityFramework;
using NBB.Invoices.Domain.InvoiceAggregate;

namespace NBB.Invoices.Data
{

    public static class DependencyInjectionExtensions
    {
        public static void AddInvoicesWriteDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfCrudRepository<Invoice, InvoicesDbContext>();

            services.AddDbContextPool<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });
        }

        public static void AddInvoicesReadDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfQuery<Invoice, InvoicesDbContext>();
            services.AddEfReadOnlyRepository<Invoice, InvoicesDbContext>();

            services.AddDbContextPool<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });
        }

        public static void AddInvoicesDataAccess(this IServiceCollection services/*, IUowPilelineBuilder uowPipelineBuilder*/)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfAsyncEnumerable<Invoice, InvoicesDbContext>();
            services.AddEfCrudRepository<Invoice, InvoicesDbContext>();

            services.AddDbContext<InvoicesDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
                });
        }
    }

    


    
}
