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

    

    public record ProjectorMetadata(Type ProjectorType, Type ModelType, Type MessageType, Type IdentityType, Type[] SubscriptionTypes, int SnapshotFrequency);

    public class ProjectorMetadataAccessor
    {
        private readonly ProjectorMetadata[] _metadata;

        public ProjectorMetadataAccessor(ProjectorMetadata[] metadata)
        {
            this._metadata = metadata;
        }

        public ProjectorMetadata GetMetadataFor<TModel>()
            => _metadata.FirstOrDefault(m => m.ModelType == typeof(TModel));
    }

    public static class ProjectorMetadataService
    {
        public static ProjectorMetadata[] ScanProjectorsMetadata(
            params Assembly[] assemblies)
            => assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(projectorType =>
                    projectorType.GetInterfaces()
                        .Where(i => i.IsGenericType && typeof(IProjector).IsAssignableFrom(i))
                        .Select(i => i.GetGenericArguments()).Select(args => (ModelType: args[0], MessageType: args[1], IdentityType:args[2]))
                        .Select(x => new ProjectorMetadata(projectorType, x.ModelType, x.MessageType, x.IdentityType, GetSubscriptionTypes(projectorType), GetSnapshotFrequency(projectorType))))
                .ToArray();

        private static Type[] GetSubscriptionTypes(Type projectorType)
        {

            var types =
                projectorType.GetInterfaces()
                    .Where(i => i.IsGenericType && typeof(ISubscribeTo).IsAssignableFrom(i))
                    .SelectMany(i => i.GetGenericArguments())
                    .Distinct()
                    .ToArray();

            return types;
        }

        private static int GetSnapshotFrequency(Type projectorType) =>
            projectorType.GetCustomAttribute<SnapshotFrequencyAttribute>()?.Frequency ?? 0;
    }
    
}
