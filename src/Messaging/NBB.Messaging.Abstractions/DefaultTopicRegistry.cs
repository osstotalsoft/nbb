// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public class DefaultTopicRegistry(IOptions<MessagingOptions> options, IConfiguration configuration) : ITopicRegistry
    {
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
            return topicNameResolver?.ResolveTopicName(messageType, configuration);
        }

        public string GetTopicPrefix()
        {
            var envPrefix = options.Value?.Env;
            if (!string.IsNullOrWhiteSpace(envPrefix))
            {
                envPrefix += ".";
            }

#pragma warning disable CS0618 // Type or member is obsolete
            var topicPrefix = envPrefix ?? options.Value?.TopicPrefix;
#pragma warning restore CS0618 // Type or member is obsolete
            return topicPrefix ?? string.Empty;
        }
    }
}
