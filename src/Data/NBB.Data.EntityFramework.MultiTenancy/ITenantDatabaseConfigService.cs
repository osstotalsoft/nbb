using System;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public interface ITenantDatabaseConfigService
    {
        string GetConnectionString(Guid tenantId);
        bool IsSharedDatabase(Guid tenantId);
    }
}