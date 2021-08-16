// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public class ImplementationTypeSelector : IImplementationTypeSelector, IMessageTypeProvider
    {
        private readonly  ITypeSourceSelector _inner;
        private readonly  IEnumerable<Type> _types;
        private readonly List<Type> _selectedTypes = new List<Type>();

        public ImplementationTypeSelector(ITypeSourceSelector inner, IEnumerable<Type> types)
        {
            _inner = inner;
            _types = types;
        }

        public ITypeSourceSelector AddClassesAssignableTo<TBase>(bool publicOnly = true)
            => SelectClasses(GetNonAbstractClasses(publicOnly).Where(t => typeof(TBase).IsAssignableFrom(t)));

        public ITypeSourceSelector AddClassesWhere(Func<Type, bool> predicate, bool publicOnly = true)
            => SelectClasses(GetNonAbstractClasses(publicOnly).Where(predicate));

        public ITypeSourceSelector AddAllClasses(bool publicOnly = true)
            => SelectClasses(GetNonAbstractClasses(publicOnly));

        IEnumerable<Type> IMessageTypeProvider.GetTypes()
            => _selectedTypes.Distinct();

        void IMessageTypeProvider.RegisterTypes(IEnumerable<Type> types)
            => SelectTypesInternal(types);

        void IMessageTypeProvider.RegisterTypes(IMessageTypeProvider provider)
            => SelectTypesInternal(provider.GetTypes());

        private ITypeSourceSelector SelectClasses(IEnumerable<Type> types)
        {
            SelectTypesInternal(types);
            return _inner;
        }

        private IEnumerable<Type> GetNonAbstractClasses(bool publicOnly)
        {
            return _types.Where(t => IsNonAbstractClass(t, publicOnly));
        }

        private static bool IsNonAbstractClass(Type type, bool publicOnly)
        {
            var typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsClass || typeInfo.IsAbstract)
                return false;

            if (typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return false;

            if (publicOnly)
                return typeInfo.IsPublic || typeInfo.IsNestedPublic;
            
            return true;
        }

        private void SelectTypesInternal(IEnumerable<Type> types)
        {
            _selectedTypes.AddRange(types);
        }
    }
}
