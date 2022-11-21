// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.MultiTenancy.Abstractions
{
    public record Tenant
    {
        public Guid TenantId { get; init; }

        public string Code { get; init; }

        public bool Enabled {get; init; }

        public Tenant() {
            Enabled = true;
         }

        public Tenant(Guid tenantId, string code, bool enabled = true)
        {
            TenantId = tenantId;
            Code = code;
            Enabled = enabled;
        }

        public bool IsValid() => TenantId != default && !string.IsNullOrWhiteSpace(Code);

        public static Tenant Default { get; } = new Tenant(default, "default");
    }
}
