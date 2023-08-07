// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace NBB.ProcessManager.Definition
{
    public interface IDefinition<TData> : IDefinition
    {
        EffectFunc<TEvent, TData> GetEffectFunc<TEvent>();
        SetStateFunc<TEvent, TData> GetSetStateFunc<TEvent>();
        Func<TEvent, object> GetCorrelationFilter<TEvent>();
        EventPredicate<TEvent, TData> GetStarterPredicate<TEvent>();
        EventPredicate<TEvent, TData> GetCompletionPredicate<TEvent>();
    }

    public interface IDefinition
    {
        IEnumerable<EventType> GetEventTypes();
    }

    public static class IDefinitionExtensions
    {
        public static bool IsObsolete(this IDefinition definition)
            => definition.GetType().HasAttribute(typeof(ObsoleteProcessAttribute));
    }

    /// <summary>
    /// New process instances cannot be started for this definition.
    /// Obsolete versions of the process definitions are retained because they are required for completing already started processes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ObsoleteProcessAttribute : Attribute
    {
    }

    public record struct EventType(Type Type, bool OnlyStartsProcess);
}
