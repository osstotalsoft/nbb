// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Data.EntityFramework;
using NBB.Payments.Domain.PayableAggregate;

namespace NBB.Payments.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddPaymentsWriteDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfCrudRepository<Payable, PaymentsDbContext>();

            services.AddDbContextPool<PaymentsDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Payments.Migrations"));
                });
        }

        public static void AddPaymentsReadDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfQuery<Payable, PaymentsDbContext>();

            services.AddDbContextPool<PaymentsDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Payments.Migrations"));
                });
        }

        public static void AddPaymentsDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfCrudRepository<Payable, PaymentsDbContext>();
            services.AddEfQuery<Payable, PaymentsDbContext>();

            services.AddDbContext<PaymentsDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Payments.Migrations"));
                });
        }
    }
}
