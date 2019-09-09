using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace NBB.Correlation.AspNet
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCorrelation(this IApplicationBuilder app)
        {
            return app.Use((context, inner) =>
            {
                Guid? ExtractGuid(StringValues values)
                {
                    if (values.Count <= 0) return null;
                    if (!Guid.TryParse(values[0], out var uuid)) return null;
            
                    return uuid;
                }

                var correlationId = 
                    ExtractGuid(context.Request.Headers[HttpRequestHeaders.CorrelationId]) ??
                    ExtractGuid(context.Request.Query["correlationId"]);

                using (CorrelationManager.NewCorrelationId(correlationId))
                {
                    return inner();
                }

            });
        }
    }
}
