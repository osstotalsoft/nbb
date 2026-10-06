// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.Todo.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Todo.Migrations
{
    public class Migrator(IServiceProvider serviceProvider, ITenantContextAccessor tenantContextAccessor, ILogger<Migrator> logger)
    {
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            // MonoTenant: the database is shared by all tenants, so migrate it in the default tenant context
            tenantContextAccessor.TenantContext = new TenantContext(Tenant.Default);

            var dbContext = serviceProvider.GetRequiredService<TodoDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database is up to date");
        }
    }
}
