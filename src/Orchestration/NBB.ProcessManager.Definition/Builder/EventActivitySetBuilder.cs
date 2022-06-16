// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using NBB.Core.Effects;
using NBB.Messaging.Effects;
using NBB.ProcessManager.Definition.SideEffects;

namespace NBB.ProcessManager.Definition.Builder
{
    public class EventActivitySetBuilder<TEvent, TData>
    {
        private readonly EventActivitySet<TEvent, TData> _eventActivitySet;

        public EventActivitySetBuilder(EventActivitySet<TEvent, TData> eventActivitySet)
        {
            _eventActivitySet = eventActivitySet;
        }

        public EventActivitySetBuilder<TEvent, TData> Then(EffectFunc<TEvent, TData> func) => Then(func, null, null);

        public EventActivitySetBuilder<TEvent, TData> ThenIf(EffectFunc<TEvent, TData> func,
            EventPredicate<TEvent, TData> predicate) => Then(func, predicate, null);

        public EventActivitySetBuilder<TEvent, TData> ThenTry(EffectFunc<TEvent, TData> func,
            Func<Exception, InstanceData<TData>, Effect<Unit>> exceptionHandler) => Then(func, null, exceptionHandler);

        public EventActivitySetBuilder<TEvent, TData> Then(EffectFunc<TEvent, TData> func,
            EventPredicate<TEvent, TData> predicate,
            Func<Exception, InstanceData<TData>, Effect<Unit>> exceptionHandler = null)
        {
            _eventActivitySet.AddEffectHandler((whenEvent, data) =>
            {
                if (predicate != null && !predicate(whenEvent, data))
                    return Effect.Pure();

                if (exceptionHandler != null)
                    return Effect.TryWith(func(whenEvent, data), ex => exceptionHandler(ex, data));

                return func(whenEvent, data);
            });
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> SetState(SetStateFunc<TEvent, TData> func,
            EventPredicate<TEvent, TData> predicate = null)
        {
            _eventActivitySet.AddSetStateHandler((whenEvent, data) =>
            {
                if (predicate != null && !predicate(whenEvent, data))
                    return data.Data;
                return func(whenEvent, data);
            });
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> SendCommand<T>(Func<TEvent, InstanceData<TData>, T> handler,
            EventPredicate<TEvent, TData> predicate = null)
        {
            Then((whenEvent, state) =>
            {
                var command = handler(whenEvent, state);
                return MessageBus.Publish(command);
            }, predicate);
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> Schedule<T>(Func<TEvent, InstanceData<TData>, T> messageFactory,
            TimeSpan timeSpan,
            EventPredicate<TEvent, TData> predicate = null)
        {
            Then(
                (whenEvent, state) =>
                    Timeout.Request(state.InstanceId.ToString(), timeSpan, messageFactory(whenEvent, state)),
                predicate);
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> PublishEvent<T>(Func<TEvent, InstanceData<TData>, T> handler,
            EventPredicate<TEvent, TData> predicate = null)
        {
            Then((whenEvent, state) =>
            {
                var @event = handler(whenEvent, state);
                return MessageBus.Publish(@event);
            }, predicate);
            return this;
        }

        public void Complete(EventPredicate<TEvent, TData> predicate = null)
        {
            _eventActivitySet.UseForCompletion(predicate);
            Then((whenEvent, state) => Timeout.Cancel(state.InstanceId.ToString()), predicate);
        }
    }
}
