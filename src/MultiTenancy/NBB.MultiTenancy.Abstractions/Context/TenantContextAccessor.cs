// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public class TenantContextAccessor : ITenantContextAccessor
    {
        private static readonly AsyncLocal<TenantContext> ContextCurrent = new AsyncLocal<TenantContext>();

        public TenantContext TenantContext
        {
            get => ContextCurrent.Value;
            set => ContextCurrent.Value = value;
        }
       
        public TenantContextFlow ChangeTenantContext(TenantContext context)
        {
            var flow = new TenantContextFlow(this, TenantContext.Clone());
            TenantContext = context;
            return flow;
        }
    }
}