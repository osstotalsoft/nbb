using System;
using Newtonsoft.Json;

namespace NBB.SQLStreamStore.Internal
{
    public class EventMetadata
    {
        public string EventTypeIdentifier { get; }
        public Guid? CorrelationId { get; }

        [JsonConstructor]
        public EventMetadata(string eventTypeIdentifier, Guid? correlationId)
        {
            EventTypeIdentifier = eventTypeIdentifier;
            CorrelationId = correlationId;
        }

        public EventMetadata(Type eventType, Guid? correlationId)
            : this(GetFullTypeName(eventType), correlationId)
        {
        }

        public Type GetEventType()
        {
            return Type.GetType(EventTypeIdentifier);
        }

        private static string GetFullTypeName(Type type)
        {
            var result = type.FullName + ", " + type.Assembly.GetName().Name;
            return result;
        }
    }
}
