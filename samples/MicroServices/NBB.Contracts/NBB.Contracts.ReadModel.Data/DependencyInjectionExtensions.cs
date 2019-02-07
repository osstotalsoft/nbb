using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Data.EntityFramework;

namespace NBB.Contracts.ReadModel.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddContractsReadModelDataAccess(this IServiceCollection services)
        {
            services.AddEntityFrameworkDataAccess();

            services.AddEfCrudRepository<ContractReadModel, ContractsReadDbContext>();
            services.AddEfQuery<ContractReadModel, ContractsReadDbContext>();

            services.AddEntityFrameworkSqlServer().AddDbContext<ContractsReadDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Contracts.Migrations"));
                });
        }
    }
}
