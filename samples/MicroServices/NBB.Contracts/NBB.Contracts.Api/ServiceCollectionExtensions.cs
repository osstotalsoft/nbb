using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Contracts.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void AddScopedContravariant<TBase, TResolve>(this IServiceCollection serviceCollection, Assembly assembly = null)
        {
            if (!typeof(TBase).IsGenericType || typeof(TBase).IsOpenGeneric())
                return;

            var baseDescription = typeof(TBase).GetGenericTypeDefinition();
            var baseInnerType = typeof(TBase).GetGenericArguments().First();

            var types = (assembly ?? baseInnerType.Assembly).ScanFor(baseInnerType);
            foreach (var t in types)
                serviceCollection.AddScoped(baseDescription.MakeGenericType(t), typeof(TResolve));
        }

        private static IEnumerable<Type> ScanFor(this Assembly assembly, Type assignableType)
        {
            return assembly.GetTypes().Where(t => assignableType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        }
    }
}
