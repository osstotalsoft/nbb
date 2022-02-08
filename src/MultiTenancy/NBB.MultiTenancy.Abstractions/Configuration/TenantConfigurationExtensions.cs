// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Configuration;

public static class TenantConfigurationExtensions
{
    public static string GetConnectionString(this ITenantConfiguration config, string name)
    {
        var splitted = config.GetValue<ConnectionStringDetails>($"ConnectionStrings:{name}");
        if (splitted != null)
        {
            return
                $"Server={splitted.Server};Database={splitted.Database};User Id={splitted.UserName};Password={splitted.Password};{splitted.OtherParams}";
        }

        return config.GetValue<string>($"ConnectionStrings:{name}");
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
