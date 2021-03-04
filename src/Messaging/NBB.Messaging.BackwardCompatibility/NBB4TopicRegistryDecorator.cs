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

            if (typeof(IRequest<Unit>).IsAssignableFrom(messageType))
            {
                topic = $"ch.commands.{topic}";
            }
            else if (typeof(INotification).IsAssignableFrom(messageType))
            {
                topic = $"ch.events.{topic}";
            }
            else
            {
                topic = $"ch.messages.{topic}";
            }
            topic = (includePrefix ? GetTopicPrefix() : string.Empty) + topic;
            return topic;
        }

        public string GetTopicForName(string topicName, bool includePrefix = true) => innerTopicRegistry.GetTopicForName(topicName, includePrefix);

        public string GetTopicPrefix() => innerTopicRegistry.GetTopicPrefix();
    }
}
