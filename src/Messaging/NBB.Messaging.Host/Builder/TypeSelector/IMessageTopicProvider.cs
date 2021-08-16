// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public interface IMessageTopicProvider
    {
        IEnumerable<string> GetTopics();
        void RegisterTopics(IEnumerable<string> topics);
    }
}
