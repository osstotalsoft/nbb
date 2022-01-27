// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.MultiTenancy.Abstractions.Configuration;
public interface ITenantConfiguration
{
    public T GetValue<T>(string key);
}
