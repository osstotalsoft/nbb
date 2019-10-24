using NBB.Core.Abstractions;
using NBB.ProcessManager.Definition.Effects;
using System;

namespace NBB.ProcessManager.Definition.Builder
{
    public class EventActivitySetBuilder<TEvent, TData>
        where TEvent : IEvent
        where TData : struct
    {
        private readonly EventActivitySet<TEvent, TData> _eventActivitySet;

        public EventActivitySetBuilder(EventActivitySet<TEvent, TData> eventActivitySet)
        {
            _eventActivitySet = eventActivitySet;
        }

        public EventActivitySetBuilder<TEvent, TData> Then(EffectHandler<TEvent, TData> handler, EventPredicate<TEvent, TData> predicate = null)
        {
            _eventActivitySet.AddEffectHandler((whenEvent, data) =>
            {
                if (predicate != null && !predicate(whenEvent, data))
                    return NoEffect.Instance;
                return handler(whenEvent, data);
            });
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> SetState(StateHandler<TEvent, TData> handler, EventPredicate<TEvent, TData> predicate = null)
        {
            _eventActivitySet.AddSetStateHandler((whenEvent, data) =>
            {
                if (predicate != null && !predicate(whenEvent, data))
                    return data.Data;
                return handler(whenEvent, data);
            });
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> SendCommand<T>(Func<TEvent, InstanceData<TData>, T> handler, EventPredicate<TEvent, TData> predicate = null)
            where T : ICommand
        {
            Then((whenEvent, state) =>
            {
                var command = handler(whenEvent, state);
                return new SendCommand(command);
            }, predicate);
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> RequestTimeout<T>(TimeSpan timeSpan, Func<TEvent, InstanceData<TData>, T> messageFactory, 
            EventPredicate<TEvent, TData> predicate = null)
            where T : IEvent
        {
            Then((whenEvent, state) => new RequestTimeout(state.CorrelationId.ToString(), timeSpan, messageFactory(whenEvent, state), typeof(T)), predicate);
            return this;
        }

        public EventActivitySetBuilder<TEvent, TData> PublishEvent<T>(Func<TEvent, InstanceData<TData>, T> handler, EventPredicate<TEvent, TData> predicate = null)
            where T : IEvent
        {
            Then((whenEvent, state) =>
            {
                var @event = handler(whenEvent, state);
                return new PublishEvent(@event);
            }, predicate);
            return this;
        }

        public void Complete(EventPredicate<TEvent, TData> predicate = null)
        {
            _eventActivitySet.UseForCompletion(predicate);
        }
    }
}