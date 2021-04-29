using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NBB.Contracts.ReadModel.Data;

namespace NBB.Contracts.Migrations
{
    //dotnet ef migrations add Initial -c ContractsReadDbContext -o ReadModelMigrations
    //dotnet ef migrations remove -c ContractsReadDbContext
    //dotnet ef database update -c ContractsReadDbContext
    public class ContractsReadDbContextFactory : IDesignTimeDbContextFactory<ContractsReadDbContext>
    {
        public ContractsReadDbContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetEntryAssembly());
            }

            var configuration = configurationBuilder.Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<ContractsReadDbContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Contracts.Migrations"));
            return new ContractsReadDbContext(builder.Options);
        }
    }
}
