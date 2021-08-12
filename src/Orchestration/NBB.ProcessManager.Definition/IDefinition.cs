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
        IEnumerable<Type> GetEventTypes();
    }
}