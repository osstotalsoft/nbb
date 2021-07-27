using System;
using System.Linq;
using System.Reflection;

namespace NBB.ProjectR
{
    
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct)
    ]
    public class SnapshotFrequencyAttribute : System.Attribute
    {
        public int Frequency { get; }

        public SnapshotFrequencyAttribute(int frequency)
        {
            Frequency = frequency;
        }
    }
    
    public interface ISubscriptionMarker { }
    public interface ISubscribeTo<T1> : ISubscriptionMarker { }
    public interface ISubscribeTo<T1, T2> : ISubscriptionMarker { }
    public interface ISubscribeTo<T1, T2, T3> : ISubscriptionMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4> : ISubscriptionMarker { }

    public record ProjectorMetadata(Type ProjectorType, Type ProjectionType, Type[] EventTypes, int SnapshotFrequency);

    public class ProjectorMetadataAccessor
    {
        private readonly ProjectorMetadata[] _metadata;

        public ProjectorMetadataAccessor(ProjectorMetadata[] metadata)
        {
            this._metadata = metadata;
        }

        public ProjectorMetadata GetMetadataFor<TProjection>()
            => this._metadata.FirstOrDefault(m => m.ProjectionType == typeof(TProjection));
    }

    public static class ProjectorMetadataService
    {
        public static ProjectorMetadata[] ScanProjectorsMetadata(
            params Assembly[] assemblies)
            => assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(projectorType =>
                    projectorType.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IProjector<>))
                        .Select(i => i.GetGenericArguments().First())
                        .Select(projectionType => new ProjectorMetadata(projectorType, projectionType, GetSubscriptionTypes(projectorType), GetSnapshotFrequency(projectorType))))
                .ToArray();
        
        private static bool IsSubscriptionMarker(Type t) =>
            typeof(ISubscriptionMarker).IsAssignableFrom(t);

        private static Type[] GetSubscriptionTypes(Type t)
        {
            if (!IsSubscriptionMarker(t))
                return Array.Empty<Type>();

            var types =
                t.GetInterfaces()
                    .Where(i => i.IsGenericType && IsSubscriptionMarker(i))
                    .SelectMany(i => i.GetGenericArguments())
                    .Distinct()
                    .ToArray();

            return types;
        }

        private static int GetSnapshotFrequency(Type t) =>
            t.GetCustomAttribute<SnapshotFrequencyAttribute>()?.Frequency ?? 0;
    }
    
}
