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
    public class TypeSourceSelector : ITypeSourceSelector, IMessageTypeProvider, IMessageTopicProvider, IServiceCollectionProvider
    {
        private readonly List<IMessageTypeProvider> _typeSelectors = new();
        private readonly IList<IEnumerable<string>> _selectedTopics = new List<IEnumerable<string>>();
        private readonly IServiceCollection _serviceCollection;

        public TypeSourceSelector(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

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

            _typeSelectors.Add(selector);

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

        IServiceCollection IServiceCollectionProvider.ServiceCollection
        {
            get => _serviceCollection;
        }

        IEnumerable<Type> IMessageTypeProvider.GetTypes()
            => _typeSelectors.SelectMany(x => x.GetTypes()).Distinct();

        void IMessageTypeProvider.RegisterTypes(IEnumerable<Type> types)
        {
            var selector = new ImplementationTypeSelector(this, types);
            _typeSelectors.Add(selector);
        }

        void IMessageTypeProvider.RegisterTypes(IMessageTypeProvider provider)
            => _typeSelectors.Add(provider);

        IEnumerable<string> IMessageTopicProvider.GetTopics()
            =>_selectedTopics.SelectMany(x => x).Distinct();

        void IMessageTopicProvider.RegisterTopics(IEnumerable<string> topics)
            => SelectTopicsInternal(topics);

        private IImplementationTypeSelector InternalFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any()) 
                throw new ArgumentException(nameof(assemblies));

            var types = assemblies.SelectMany(asm => asm.DefinedTypes).Select(x => x.AsType());
            var selector = new ImplementationTypeSelector(this, types);
            _typeSelectors.Add(selector);

            return selector;
        }

        private void SelectTopicsInternal(IEnumerable<string> topics)
        {
            _selectedTopics.Add(topics.ToList());
        }
    }
}
