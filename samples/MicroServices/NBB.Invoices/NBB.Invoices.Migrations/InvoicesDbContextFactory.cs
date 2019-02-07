using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NBB.Invoices.Data;

namespace NBB.Invoices.Migrations
{
    //dotnet ef migrations add Initial -c InvoicesDbContext -o Migrations
    //dotnet ef migrations remove -c InvoicesDbContext
    //dotnet ef database update -c InvoicesDbContext
    public class InvoicesDbContextFactory : IDesignTimeDbContextFactory<InvoicesDbContext>
    {
        public InvoicesDbContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<InvoicesDbContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Invoices.Migrations"));
            return new InvoicesDbContext(builder.Options);
        }
    }
}
