// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
