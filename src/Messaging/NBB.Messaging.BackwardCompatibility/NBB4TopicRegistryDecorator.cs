using NBB.Messaging.Abstractions;
using System;
using MediatR;

namespace NBB.Messaging.BackwardCompatibility
{
    public class NBB4TopicRegistryDecorator : ITopicRegistry
    {
        private readonly ITopicRegistry innerTopicRegistry;

        public NBB4TopicRegistryDecorator(ITopicRegistry innerTopicRegistry)
        {
            this.innerTopicRegistry = innerTopicRegistry;
        }

        public string GetTopicForMessageType(Type messageType, bool includePrefix = true)
        {
            var topic = innerTopicRegistry.GetTopicForMessageType(messageType, false);

            static string Prepend(string prefix, string str)
                => str.StartsWith(prefix) ? str : $"{prefix}{str}";

            if (typeof(IRequest<Unit>).IsAssignableFrom(messageType))
            {
                topic = Prepend("ch.commands.", topic);
            }
            else if (typeof(INotification).IsAssignableFrom(messageType))
            {
                topic = Prepend("ch.events.", topic);
            }
            else
            {
                topic = Prepend("ch.messages.", topic);
            }
            topic = (includePrefix ? GetTopicPrefix() : string.Empty) + topic;
            return topic;
        }

        public string GetTopicForName(string topicName, bool includePrefix = true) => innerTopicRegistry.GetTopicForName(topicName, includePrefix);

        public string GetTopicPrefix() => innerTopicRegistry.GetTopicPrefix();
    }
}
