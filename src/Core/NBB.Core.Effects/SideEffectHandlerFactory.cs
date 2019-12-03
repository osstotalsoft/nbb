using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.Effects
{
    public class SideEffectHandlerFactory : ISideEffectHandlerFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> Cache = new ConcurrentDictionary<Type, Type>();
        private readonly IServiceProvider _serviceProvider;

        public SideEffectHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISideEffectHandler<ISideEffect<TOutput>, TOutput> GetSideEffectHandlerFor<TOutput>(ISideEffect<TOutput> sideEffect)
        {
            var sideEffectHandlerType = GetSideEffectHandlerTypeFor(sideEffect.GetType());
            var sideEffectHandler = _serviceProvider.GetRequiredService(sideEffectHandlerType) as ISideEffectHandler;

            if (sideEffectHandler == null)
            {
                throw new Exception($"Could not create a side effect handler for type {sideEffectHandlerType.Name}");
            }
            return new SideEffectHandlerWrapper<TOutput>(sideEffectHandler);
        }


        private Type GetSideEffectHandlerTypeFor(Type sideEffectType)
        {
            var handlerType = Cache.GetOrAdd(sideEffectType, sideEffType =>
            {
                if (TypeImplementsOpenGenericInterface(sideEffType, typeof(IAmHandledBy<>)))
                {
                    return GetFirstTypeParamForOpenGenericInterface(sideEffType, typeof(IAmHandledBy<>));
                }

                var outputType = GetFirstTypeParamForOpenGenericInterface(sideEffectType, typeof(ISideEffect<>));
                return typeof(ISideEffectHandler<,>).MakeGenericType(sideEffType, outputType);
            });

            return handlerType;
        }

        private bool TypeImplementsOpenGenericInterface(Type t, Type openGenericInterfaceType)
        {
            return t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceType);
        }

        private Type GetFirstTypeParamForOpenGenericInterface(Type genericType, Type openGenericInterfaceType)
        {
            var closedGenericIntf = genericType.GetInterfaces().SingleOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceType);

            if (closedGenericIntf == null)
            {
                throw new Exception($"Type {genericType.Name} does not implement generic interface {openGenericInterfaceType.Name}");
            }

            return closedGenericIntf.GetGenericArguments().First();
        }
    }
}
