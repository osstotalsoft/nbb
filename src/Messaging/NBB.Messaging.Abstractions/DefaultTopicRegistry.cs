using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public class DefaultTopicRegistry : ITopicRegistry
    {
        private readonly IConfiguration _configuration;

        public DefaultTopicRegistry(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetTopicForMessageType(Type messageType, bool includePrefix = true)
        {
            var topic = GetTopicNameFromAttribute(messageType);
            if (topic == null)
            {
                if (typeof(ICommand).IsAssignableFrom(messageType))
                {
                    topic = $"ch.commands.{messageType.GetLongPrettyName()}";
                }
                else if (typeof(IEvent).IsAssignableFrom(messageType))
                {
                    topic = $"ch.events.{messageType.GetLongPrettyName()}";
                }
                else if (typeof(IQuery).IsAssignableFrom(messageType))
                {
                    topic = $"ch.queries.{messageType.GetLongPrettyName()}";
                }
                else
                {
                    topic = $"ch.messages.{messageType.GetLongPrettyName()}";
                }
            }

            
            topic = GetTopicForName(topic, includePrefix);

            return topic;
        }

        public string GetTopicForName(string topicName, bool includePrefix = true)
        {
            if (topicName == null)
            {
                return null;
            }

            var topic = (includePrefix ? GetTopicPrefix() : string.Empty) + topicName;
            topic = topic.Replace("+", ".");
            topic = topic.Replace("<", "_");
            topic = topic.Replace(">", "_");

            return topic;
        }


        private string GetTopicNameFromAttribute(Type messageType)
        {
            var topicNameResolver = messageType.GetCustomAttributes(typeof(TopicNameResolverAttribute), true).FirstOrDefault() as TopicNameResolverAttribute;
            return topicNameResolver?.ResolveTopicName(messageType, _configuration);
        }

        public string GetTopicPrefix()
        {
            var topicPrefix = _configuration.GetSection("Messaging")?["TopicPrefix"];
            return topicPrefix ?? "";
        }
    }
}