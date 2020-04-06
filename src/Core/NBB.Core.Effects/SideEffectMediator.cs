using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class SideEffectMediator : ISideEffectMediator
    {
        private static readonly ConcurrentDictionary<Type, Type> Cache = new ConcurrentDictionary<Type, Type>();
        private readonly IServiceProvider _serviceProvider;

        public SideEffectMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<T> Run<T>(ISideEffect<T> sideEffect, CancellationToken cancellationToken = default)
        {
            var sideEffectHandlerType = GetSideEffectHandlerTypeFor(sideEffect.GetType());
            var sideEffectHandler = _serviceProvider.GetRequiredService(sideEffectHandlerType) as ISideEffectHandler;
            if (sideEffectHandler == null)
            {
                throw new Exception($"Could not create a side effect handler for type {sideEffectHandlerType.Name}");
            }
            var mi = sideEffectHandler.GetType().GetMethod("Handle");
            var task = mi.Invoke(sideEffectHandler, new object[] { sideEffect, cancellationToken }) as Task;
            //var task = sideEffectHandler.AsDynamic().Handle((sideEffect as dynamic), cancellationToken);
            if (typeof(T) == typeof(Unit))
            {
                await task;
                return Unit.Value as dynamic;
            }
            else
            {
                var x = await (task as Task<T>);
                return x;
            }
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
                return outputType == typeof(Unit)
                    ? typeof(ISideEffectHandler<>).MakeGenericType(sideEffType)
                    : typeof(ISideEffectHandler<,>).MakeGenericType(sideEffType, outputType);
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
