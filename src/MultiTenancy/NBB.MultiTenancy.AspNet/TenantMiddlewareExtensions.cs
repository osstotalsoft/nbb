using Microsoft.AspNetCore.Builder;

namespace NBB.MultiTenancy.AspNet
{
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantContextMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TenantMiddleware>();
        }
    }
}
