using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectR
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddProjectR(IServiceCollection services, params Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IProject<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ICorrelate<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
                


            var metadata =
                assemblies
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t =>
                        t.GetInterfaces()
                            .Where(i =>
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IProject<,>))
                            .Select(i =>
                            {
                                var args = i.GetGenericTypeDefinition().GenericTypeArguments;
                                var eventType = args[0];
                                var projectionType = args[1];
                                var identityType = GetIdentityTypeFrom(projectionType);
                                return new {eventType, projectionType, identityType};
                            }))
                    .ToList();
            
            foreach (var m in metadata)
            {
                var serviceType = typeof(IEventProjector<>).MakeGenericType(m.eventType);
                var implementationType =
                    typeof(EventProjector<,,>).MakeGenericType(m.eventType, m.projectionType, m.identityType);
                services.AddScoped(serviceType, implementationType);
            }

            services.AddScoped<IProjector, Projector>();


            return services;
        }

        private static Type GetIdentityTypeFrom(Type projectionType)
        {
            var identityMarkerInterface =
                projectionType
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHaveIdentityOf<>));
            
             return identityMarkerInterface?.GetGenericArguments().First();
        }


    }
}
