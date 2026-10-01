// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public class TypeSourceSelector(IServiceCollection serviceCollection) : ITypeSourceSelector, IMessageTypeProvider, IMessageTopicProvider, IServiceCollectionProvider
    {
        // shared with the selectors created by FromServiceCollection, so their types and topics belong to the same subscriber group
        private List<IMessageTypeProvider> TypeSelectors { get; init; } = [];
        private IList<IEnumerable<string>> SelectedTopics { get; init; } = [];

        public IImplementationTypeSelector FromAssemblyOf<T>()
            => InternalFromAssembliesOf(new[] {typeof(T).GetTypeInfo()});

        public IImplementationTypeSelector FromCallingAssembly()
            =>FromAssemblies(Assembly.GetCallingAssembly());

        public IImplementationTypeSelector FromExecutingAssembly()
            => FromAssemblies(Assembly.GetExecutingAssembly());

        public IImplementationTypeSelector FromEntryAssembly()
            => FromAssemblies(Assembly.GetEntryAssembly());

        public IImplementationTypeSelector FromAssembliesOf(params Type[] types)
            => InternalFromAssembliesOf(types.Select(x => x.GetTypeInfo()));

        public IImplementationTypeSelector FromAssembliesOf(IEnumerable<Type> types) 
            => InternalFromAssembliesOf(types.Select(t => t.GetTypeInfo()));

        public IImplementationTypeSelector FromAssemblies(params Assembly[] assemblies)
            => FromAssemblies(assemblies.AsEnumerable());

        public IImplementationTypeSelector FromAssemblies(IEnumerable<Assembly> assemblies)
            => InternalFromAssemblies(assemblies);

        private IImplementationTypeSelector InternalFromAssembliesOf(IEnumerable<TypeInfo> typeInfos)
            => InternalFromAssemblies(typeInfos.Select(t => t.Assembly));

        public ITypeSourceSelector AddType<TMessage>()
            => AddTypes(new[] {typeof(TMessage)});

        public ITypeSourceSelector AddTypes(params Type[] types)
            => AddTypes(types.AsEnumerable());

        public ITypeSourceSelector AddTypes(IEnumerable<Type> types)
        {
            if (types == null || !types.Any())
                throw new ArgumentException(nameof(types));

            var selector = new ImplementationTypeSelector(this, types);

            TypeSelectors.Add(selector);

            return selector.AddAllClasses();
        }
        public ITypeSourceSelector FromTopic(string topic)
            => FromTopics(new[] {topic});

        public ITypeSourceSelector FromTopics(params string[] topics)
            => FromTopics(topics.AsEnumerable());

        public ITypeSourceSelector FromTopics(IEnumerable<string> topics)
        {
            SelectTopicsInternal(topics);
            return this;
        }

        public ITypeSourceSelector FromServiceCollection(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            return new TypeSourceSelector(services) { TypeSelectors = TypeSelectors, SelectedTopics = SelectedTopics };
        }

        IServiceCollection IServiceCollectionProvider.ServiceCollection
        {
            get => serviceCollection;
        }

        IEnumerable<Type> IMessageTypeProvider.GetTypes()
            => TypeSelectors.SelectMany(x => x.GetTypes()).Distinct();

        void IMessageTypeProvider.RegisterTypes(IEnumerable<Type> types)
        {
            var selector = new ImplementationTypeSelector(this, types);
            TypeSelectors.Add(selector);
        }

        void IMessageTypeProvider.RegisterTypes(IMessageTypeProvider provider)
            => TypeSelectors.Add(provider);

        IEnumerable<string> IMessageTopicProvider.GetTopics()
            =>SelectedTopics.SelectMany(x => x).Distinct();

        void IMessageTopicProvider.RegisterTopics(IEnumerable<string> topics)
            => SelectTopicsInternal(topics);

        private IImplementationTypeSelector InternalFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any()) 
                throw new ArgumentException(nameof(assemblies));

            var types = assemblies.SelectMany(asm => asm.DefinedTypes).Select(x => x.AsType());
            var selector = new ImplementationTypeSelector(this, types);
            TypeSelectors.Add(selector);

            return selector;
        }

        private void SelectTopicsInternal(IEnumerable<string> topics)
        {
            SelectedTopics.Add(topics.ToList());
        }
    }
}
