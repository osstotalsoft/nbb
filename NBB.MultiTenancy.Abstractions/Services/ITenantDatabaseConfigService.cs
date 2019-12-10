using System;

namespace NBB.MultiTenancy.Abstractions.Services
{
    public interface ITenantDatabaseConfigService
    {
        string GetConnectionString(Guid tenantId);
        bool IsSharedDatabase(Guid tenantId);
    }
}