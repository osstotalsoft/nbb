// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
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

            var tenenantContextAccessor = dependencyResolver.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenenantContextAccessor.TenantContext = new TenantContext(Tenant.Default);

            return dependencyResolver.ServiceProvider.GetRequiredService<TodoDbContext>();
        }
    }
}
