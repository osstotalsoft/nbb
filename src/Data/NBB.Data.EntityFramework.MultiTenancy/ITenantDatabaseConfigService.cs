// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public interface ITenantDatabaseConfigService
    {
        string GetConnectionString();
    }
}