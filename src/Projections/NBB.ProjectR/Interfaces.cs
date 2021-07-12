using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public interface IHaveIdentityOf<TIdentity> { }

    public interface IProjector<TProjection>
    {
        (TProjection Projection, Effect<Unit> Effect) Project(object ev, TProjection projection);
        object Correlate(object ev);
    }


    public interface IProjectionStore<TProjection>
    {
        Task<(TProjection Projection, int Version)> Load(object id, CancellationToken cancellationToken);
        Task SaveEvent<TEvent>(TEvent ev, object id, int expectedVersion, CancellationToken cancellationToken);
    }

    public interface ISubscriptionMarker { }
    public interface ISubscribeTo<T1> : ISubscriptionMarker { }
    public interface ISubscribeTo<T1, T2> : ISubscriptionMarker { }
    public interface ISubscribeTo<T1, T2, T3> : ISubscriptionMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4> : ISubscriptionMarker { }
    
    public static class SubscriptionMarkerExtensions
    {
        public static bool IsSubscriptionMarker(this Type t) =>
            typeof(ISubscriptionMarker).IsAssignableFrom(t);

        public static Type[] GetSubscriptionTypes(this Type t)
        {
            if (!t.IsSubscriptionMarker())
                return Array.Empty<Type>();

            var types =
                t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.IsSubscriptionMarker())
                    .SelectMany(i => i.GetGenericArguments())
                    .Distinct()
                    .ToArray();

            return types;
        }
    }


}
