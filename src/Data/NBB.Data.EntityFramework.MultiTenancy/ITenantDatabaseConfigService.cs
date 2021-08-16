// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public interface ITenantDatabaseConfigService
    {
        string GetConnectionString();
    }
}