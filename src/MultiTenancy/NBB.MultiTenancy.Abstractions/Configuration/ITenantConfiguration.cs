// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.MultiTenancy.Abstractions.Configuration;
public interface ITenantConfiguration
{
    /// <summary>
    /// Extracts the value with the specified key and converts it to type T.
    /// or
    /// Attempts to bind the configuration instance to a new instance of type T.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    public T GetValue<T>(string key);
}
