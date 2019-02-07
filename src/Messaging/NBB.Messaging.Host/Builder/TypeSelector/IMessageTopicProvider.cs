using System.Collections.Generic;

namespace NBB.Messaging.Host.Builder.TypeSelector
{
    public interface IMessageTopicProvider
    {
        IEnumerable<string> GetTopics();
        void RegisterTopics(IEnumerable<string> topics);
    }
}
