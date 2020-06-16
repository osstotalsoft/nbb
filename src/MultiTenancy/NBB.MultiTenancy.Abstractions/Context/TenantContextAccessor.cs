using System.Threading;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public class TenantContextAccessor : ITenantContextAccessor
    {
        private static readonly AsyncLocal<ContextHolder> ContextCurrent = new AsyncLocal<ContextHolder>();

        private class ContextHolder
        {
            public TenantContext Context;
        }

        public TenantContext TenantContext
        {
            get => ContextCurrent.Value?.Context;
            set
            {
                var holder = ContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current TenantContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the TenantContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    ContextCurrent.Value = new ContextHolder {Context = value};
                }
            }
        }

        public TenantContextFlow ChangeTenantContext(TenantContext context)
        {
            var flow = new TenantContextFlow(this, TenantContext.Clone());
            TenantContext = context;
            return flow;
        }
    }
}