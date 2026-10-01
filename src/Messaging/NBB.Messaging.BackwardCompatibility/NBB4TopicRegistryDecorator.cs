// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using System;

namespace NBB.Messaging.BackwardCompatibility
{
    public class NBB4TopicRegistryDecorator(ITopicRegistry innerTopicRegistry, IContractKindClassifier contractKindClassifier) : ITopicRegistry
    {
        public string GetTopicForMessageType(Type messageType, bool includePrefix = true)
        {
            var topic = innerTopicRegistry.GetTopicForMessageType(messageType, false);

            static string Prepend(string prefix, string str)
                => str.StartsWith(prefix) ? str : $"{prefix}{str}";

            // NBB 4 topics: queries and other types share the "ch.messages." prefix
            topic = contractKindClassifier.Classify(messageType) switch
            {
                ContractKind.Command => Prepend("ch.commands.", topic),
                ContractKind.Event => Prepend("ch.events.", topic),
                _ => Prepend("ch.messages.", topic)
            };
            topic = (includePrefix ? GetTopicPrefix() : string.Empty) + topic;
            return topic;
        }

        public string GetTopicForName(string topicName, bool includePrefix = true) => innerTopicRegistry.GetTopicForName(topicName, includePrefix);

        public string GetTopicPrefix() => innerTopicRegistry.GetTopicPrefix();
    }
}
