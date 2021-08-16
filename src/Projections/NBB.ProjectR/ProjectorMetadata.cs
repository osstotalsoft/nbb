// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;

namespace NBB.ProjectR
{
    
    public interface ISubscribeToMarker { }
    public interface ISubscribeTo<T1> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2, T3> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4, T5> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4, T5, T6> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4, T5, T6, T7> : ISubscribeToMarker { }
    public interface ISubscribeTo<T1, T2, T3, T4, T5, T6, T7, T8> : ISubscribeToMarker { }


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
                        .Where(i => i.IsGenericType && typeof(IProjectorMarker).IsAssignableFrom(i))
                        .Select(i => i.GetGenericArguments()).Select(args => (ModelType: args[0], MessageType: args[1], IdentityType:args[2]))
                        .Select(x => new ProjectorMetadata(projectorType, x.ModelType, x.MessageType, x.IdentityType, GetSubscriptionTypes(projectorType), GetSnapshotFrequency(projectorType))))
                .ToArray();

        private static Type[] GetSubscriptionTypes(Type projectorType)
        {

            var types =
                projectorType.GetInterfaces()
                    .Where(i => i.IsGenericType && typeof(ISubscribeToMarker).IsAssignableFrom(i))
                    .SelectMany(i => i.GetGenericArguments())
                    .Distinct()
                    .ToArray();

            return types;
        }

        private static int GetSnapshotFrequency(Type projectorType) =>
            projectorType.GetCustomAttribute<SnapshotFrequencyAttribute>()?.Frequency ?? 0;
    }
    
}
