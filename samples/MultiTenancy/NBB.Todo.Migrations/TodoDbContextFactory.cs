using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.Todo.Data;

namespace NBB.Todo.Migrations
{
    //dotnet ef migrations add Initial -c TodoDbContext -o Migrations
    //dotnet ef migrations remove -c TodoDbContext
    public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
    {
        public TodoDbContext CreateDbContext(string[] args)
        {
            var dependencyResolver = new DependencyResolver();

            var tenancyOptions = dependencyResolver.ServiceProvider.GetRequiredService<IOptions<TenancyHostingOptions>>();
            var isMultiTenant = tenancyOptions?.Value?.TenancyType != TenancyType.None;

            if (isMultiTenant)
            {
                var tenenantContextAccessor = dependencyResolver.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
                tenenantContextAccessor.TenantContext = new TenantContext(new Tenant(Guid.NewGuid(), string.Empty, true));
            }

            return dependencyResolver.ServiceProvider.GetRequiredService<TodoDbContext>();
        }
    }
}
