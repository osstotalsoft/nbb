using System;

namespace NBB.MultiTenancy.Data.Abstractions
{
    public interface ITenantDatabaseConfigService
    {
        string GetConnectionString(Guid tenantId);
        bool IsSharedDatabase(Guid tenantId);
    }
}