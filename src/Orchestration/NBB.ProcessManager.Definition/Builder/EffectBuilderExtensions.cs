// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Effects;
using NBB.ProcessManager.Definition.SideEffects;
using System;

namespace NBB.ProcessManager.Definition.Builder
{
    public static class EffectBuilderExtensions
    {
        public static IEventActivitySetEffectBuilder<TEvent, TData> SendCommand<T, TEvent, TData>(this IEventActivitySetEffectBuilder<TEvent, TData> builder,
            Func<TEvent, InstanceData<TData>, T> handler, EventPredicate<TEvent, TData> predicate = null)
        {
            builder.Then((whenEvent, state) =>
            {
                var command = handler(whenEvent, state);
                return MessageBus.Publish(command);
            }, predicate);
            return builder;
        }

        public static IEventActivitySetEffectBuilder<TEvent, TData> Schedule<T, TEvent, TData>(this IEventActivitySetEffectBuilder<TEvent, TData> builder,
            Func<TEvent, InstanceData<TData>, T> messageFactory, TimeSpan timeSpan,
            EventPredicate<TEvent, TData> predicate = null)
        {
            builder.Then((whenEvent, state) => Timeout.Request(state.InstanceId.ToString(), timeSpan, messageFactory(whenEvent, state)), predicate);
            return builder;
        }

        public static IEventActivitySetEffectBuilder<TEvent, TData> PublishEvent<T, TEvent, TData>(this IEventActivitySetEffectBuilder<TEvent, TData> builder,
            Func<TEvent, InstanceData<TData>, T> handler, EventPredicate<TEvent, TData> predicate = null)
        {
            builder.Then((whenEvent, state) =>
            {
                var @event = handler(whenEvent, state);
                return MessageBus.Publish(@event);
            }, predicate);
            return builder;
        }
 
    }
}
