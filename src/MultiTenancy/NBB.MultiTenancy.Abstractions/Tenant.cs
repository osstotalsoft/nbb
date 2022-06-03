// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.MultiTenancy.Abstractions
{
    public record Tenant
    {
        public Guid TenantId { get; init; }

        public string Code { get; init; }

        public Tenant() { }

        public Tenant(Guid tenantId, string code)
        {
            TenantId = tenantId;
            Code = code;
        }

        public bool IsValid() => TenantId != default && !string.IsNullOrWhiteSpace(Code);

        public static Tenant Default { get; } = new Tenant(default, "default");
    }
}
