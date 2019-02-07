//using System;
//using NBB.BuildingBlocks.Core.Abstractions;

//namespace NBB.BuildingBlocks.EventStore.Abstractions
//{
//    public class EventStoreEnvelope : IEvent
//    {
//        public IEvent Event { get; protected set; }

//        Guid IEvent.EventId => Event.EventId;
//        Guid? IEvent.CorrelationId => Event.CorrelationId;
//    }


//    public class EventStoreEnvelope<TEvent> : EventStoreEnvelope
//        where TEvent : class, IEvent
//    {
//        public new TEvent Event
//        {
//            get => base.Event as TEvent;
//            set => base.Event = value;
//        }


//        public EventStoreEnvelope(TEvent @event)
//        {
//            Event = @event;
//        }
//    }

//    public static class EventStoreEnvelopeExtensions
//    {
//        public static EventStoreEnvelope EnvelopeAsEventStoreEvent<TEvent>(this TEvent @event)
//            where TEvent : IEvent
//        {
//            var type = typeof(EventStoreEnvelope<>).MakeGenericType(@event.GetType());
//            var envelopedEvent = Activator.CreateInstance(type, @event) as EventStoreEnvelope;
//            return envelopedEvent;
//        }
//    }
//}
