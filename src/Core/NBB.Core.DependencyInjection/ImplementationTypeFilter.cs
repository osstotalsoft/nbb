// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Scrutor;

namespace NBB.Core.DependencyInjection
{
    internal class ImplementationTypeFilter : IImplementationTypeFilter
    {
        public ImplementationTypeFilter(IEnumerable<Type> types)
        {
            Types = types;
        }

        internal IEnumerable<Type> Types { get; private set; }

        public IImplementationTypeFilter AssignableTo<T>()
        {
            return AssignableTo(typeof(T));
        }

        public IImplementationTypeFilter AssignableTo(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return AssignableToAny(type);
        }

        public IImplementationTypeFilter AssignableToAny(params Type[] types)
        {
            ArgumentNullException.ThrowIfNull(types);

            return AssignableToAny(types.AsEnumerable());
        }

        public IImplementationTypeFilter AssignableToAny(IEnumerable<Type> types)
        {
            ArgumentNullException.ThrowIfNull(types);

            return Where(t => types.Any(t.IsAssignableTo));
        }

        public IImplementationTypeFilter WithAttribute<T>() where T : Attribute
        {
            return WithAttribute(typeof(T));
        }

        public IImplementationTypeFilter WithAttribute(Type attributeType)
        {
            ArgumentNullException.ThrowIfNull(attributeType);

            return Where(t => t.HasAttribute(attributeType));
        }

        public IImplementationTypeFilter WithAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return Where(t => t.HasAttribute(predicate));
        }

        public IImplementationTypeFilter WithoutAttribute<T>() where T : Attribute
        {
            return WithoutAttribute(typeof(T));
        }

        public IImplementationTypeFilter WithoutAttribute(Type attributeType)
        {
            ArgumentNullException.ThrowIfNull(attributeType);

            return Where(t => !t.HasAttribute(attributeType));
        }

        public IImplementationTypeFilter WithoutAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return Where(t => !t.HasAttribute(predicate));
        }

        public IImplementationTypeFilter InNamespaceOf<T>()
        {
            return InNamespaceOf(typeof(T));
        }

        public IImplementationTypeFilter InNamespaceOf(params Type[] types)
        {
            ArgumentNullException.ThrowIfNull(types);

            return InNamespaces(types.Select(t => t.Namespace));
        }

        public IImplementationTypeFilter InNamespaces(params string[] namespaces)
        {
            ArgumentNullException.ThrowIfNull(namespaces);

            return InNamespaces(namespaces.AsEnumerable());
        }

        public IImplementationTypeFilter InNamespaces(IEnumerable<string> namespaces)
        {
            ArgumentNullException.ThrowIfNull(namespaces);

            return Where(t => namespaces.Any(t.IsInNamespace));
        }

        public IImplementationTypeFilter NotInNamespaceOf<T>()
        {
            return NotInNamespaceOf(typeof(T));
        }

        public IImplementationTypeFilter NotInNamespaceOf(params Type[] types)
        {
            ArgumentNullException.ThrowIfNull(types);

            return NotInNamespaces(types.Select(t => t.Namespace));
        }

        public IImplementationTypeFilter NotInNamespaces(params string[] namespaces)
        {
            ArgumentNullException.ThrowIfNull(namespaces);

            return NotInNamespaces(namespaces.AsEnumerable());
        }

        public IImplementationTypeFilter NotInNamespaces(IEnumerable<string> namespaces)
        {
            ArgumentNullException.ThrowIfNull(namespaces);

            return Where(t => namespaces.All(ns => !t.IsInNamespace(ns)));
        }

        public IImplementationTypeFilter Where(Func<Type, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            Types = Types.Where(predicate);
            return this;
        }

        public IImplementationTypeFilter InExactNamespaceOf<T>()
        {
            return InExactNamespaceOf(typeof(T));
        }

        public IImplementationTypeFilter InExactNamespaceOf(params Type[] types)
        {
            ArgumentNullException.ThrowIfNull(types);
            return Where(t => types.Any(x => t.IsInExactNamespace(x.Namespace)));
        }

        public IImplementationTypeFilter InExactNamespaces(params string[] namespaces)
        {
            ArgumentNullException.ThrowIfNull(namespaces);

            return Where(t => namespaces.Any(t.IsInExactNamespace));
        }
    }
}