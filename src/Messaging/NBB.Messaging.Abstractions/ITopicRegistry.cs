using System;

namespace NBB.Messaging.Abstractions
{
    public interface ITopicRegistry
    {
        string GetTopicForMessageType(Type messageType, bool includePrefix = true);
        string GetTopicForName(string topicName, bool includePrefix = true);
        string GetTopicPrefix();
    }
}
