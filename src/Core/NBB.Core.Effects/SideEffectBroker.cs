using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class SideEffectBroker : ISideEffectBroker
    {
        private static readonly ConcurrentDictionary<Type, Type> Cache = new();
        private readonly IServiceProvider _serviceProvider;

        public SideEffectBroker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TSideEffectResult> Run<TSideEffect, TSideEffectResult>(TSideEffect sideEffect, CancellationToken cancellationToken = default)
            where TSideEffect : ISideEffect<TSideEffectResult>
        {
            var sideEffectHandlerType = GetSideEffectHandlerTypeFor<TSideEffect, TSideEffectResult>();
            if (_serviceProvider.GetRequiredService(sideEffectHandlerType) is not ISideEffectHandler<TSideEffect, TSideEffectResult> sideEffectHandler)
            {
                throw new Exception($"Could not create a side effect handler for type {typeof(TSideEffect).Name}");
            }

            var result = await sideEffectHandler.Handle(sideEffect, cancellationToken);
            return result;
        }


        private static Type GetSideEffectHandlerTypeFor<TSideEffect, TSideEffectResult>()
            where TSideEffect : ISideEffect<TSideEffectResult>
        {
            var handlerType = Cache.GetOrAdd(
                typeof(TSideEffect),
                sideEffType => TypeImplementsOpenGenericInterface(sideEffType, typeof(IAmHandledBy<>))
                    ? GetFirstTypeParamForOpenGenericInterface(sideEffType, typeof(IAmHandledBy<>))
                    : typeof(ISideEffectHandler<TSideEffect, TSideEffectResult>));

            return handlerType;
        }

        private static bool TypeImplementsOpenGenericInterface(Type t, Type openGenericInterfaceType)
        {
            return t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceType);
        }

        private static Type GetFirstTypeParamForOpenGenericInterface(Type genericType, Type openGenericInterfaceType)
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
