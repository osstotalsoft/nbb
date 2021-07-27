using System;
using System.Linq;
using System.Reflection;
using NBB.Core.Effects;

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
                        .Where(i => i.IsGenericType && typeof(IProjector).IsAssignableFrom(i) && i.GenericTypeArguments.Length > 1)
                        .Select(i => i.GetGenericArguments().First())
                        .Select(projectionType => new ProjectorMetadata(projectorType, projectionType, GetSubscriptionTypes(projectorType), GetSnapshotFrequency(projectorType))))
                .ToArray();

        private static Type[] GetSubscriptionTypes(Type projectorType)
        {

            var types =
                projectorType.GetInterfaces()
                    .Where(i => i.IsGenericType && typeof(IProjector).IsAssignableFrom(i) && i.GenericTypeArguments.Length > 1)
                    .SelectMany(i => i.GetGenericArguments().Skip(1))
                    .Distinct()
                    .ToArray();

            return types;
        }

        private static int GetSnapshotFrequency(Type projectorType) =>
            projectorType.GetCustomAttribute<SnapshotFrequencyAttribute>()?.Frequency ?? 0;
    }
    
}
