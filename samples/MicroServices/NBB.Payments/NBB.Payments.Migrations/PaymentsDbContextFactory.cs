using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NBB.Payments.Data;

namespace NBB.Payments.Migrations
{
    //dotnet ef migrations add Initial -c PaymentsDbContext -o Migrations
    //dotnet ef migrations remove -c PaymentsDbContext
    //dotnet ef database update -c PaymentsDbContext
    public class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
    {
        public PaymentsDbContext CreateDbContext(string[] args)
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

            var builder = new DbContextOptionsBuilder<PaymentsDbContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Payments.Migrations"));
            return new PaymentsDbContext(builder.Options);
        }
    }
}
