// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace NBB.MultiTenancy.Abstractions.Configuration;

public static class TenantConfigurationExtensions
{
    /// <summary>
    /// Extracts the value with the specified key and converts it to type T.
    /// or
    /// Attempts to bind the configuration instance to a new instance of type T.
    /// If this configuration section has a value, that will be used.
    /// Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    public static T GetValue<T>(this ITenantConfiguration config, string key)
    {
        return getValueOrComplexObject<T>(config, key);
    }

    private static T getValueOrComplexObject<T>(IConfiguration config, string key)
    {
        //section.GetSection is never null
        if (config.GetSection(key).GetChildren().Any())
        {
            //complex type is present
            return config.GetSection(key).Get<T>();
        }

        if (config.GetSection(key).Value != null)
            return config.GetValue<T>(key);

        return default;
    }

    /// <summary>
    /// Retrieves connection string from separate connection info segments or string value
    /// </summary>
    /// <param name="config"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetConnectionString(this ITenantConfiguration config, string name)
    {
        var splitted = config.GetValue<ConnectionStringDetails>($"ConnectionStrings:{name}");
        if (splitted != null)
        {
            CheckMandatoryField(splitted.Server, nameof(splitted.Server));
            CheckMandatoryField(splitted.Database, nameof(splitted.Database));
            CheckMandatoryField(splitted.UserName, nameof(splitted.UserName));
            CheckMandatoryField(splitted.Password, nameof(splitted.Password));

            return
                $"Server={splitted.Server};Database={splitted.Database};User Id={splitted.UserName};Password={splitted.Password};{splitted.OtherParams}";
        }

        return config.GetValue<string>($"ConnectionStrings:{name}");
    }

    private static void CheckMandatoryField(string fieldValue, string fieldName) {
        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            throw new ArgumentException($"Connection string part {fieldName} is not provided!");
        }
    }


    private class ConnectionStringDetails
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OtherParams { get; set; }
    }
}
