// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
            var topic = GetTopicNameFromAttribute(messageType) ?? messageType.GetLongPrettyName();

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
            var messagingSection = _configuration.GetSection("Messaging");
            var envPrefix = messagingSection?["Env"];
            if (!string.IsNullOrWhiteSpace(envPrefix))
            {
                envPrefix += ".";
            }

            var topicPrefix = envPrefix ?? messagingSection?["TopicPrefix"];
            return topicPrefix ?? string.Empty;
        }
    }
}
