// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Configuration
{
    public static class TenantConfigurationExtensions
    {
        public static string GetConnectionString(this ITenantConfiguration config, string name)
        {
            return config.GetValue<string>($"ConnectionStrings:{name}");
        }
    }
}
