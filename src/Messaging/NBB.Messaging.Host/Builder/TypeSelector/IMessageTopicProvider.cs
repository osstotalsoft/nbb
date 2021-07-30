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
