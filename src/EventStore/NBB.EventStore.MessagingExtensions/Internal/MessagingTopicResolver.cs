using Microsoft.Extensions.Options;
using NBB.EventStore.Abstractions;

namespace NBB.EventStore.MessagingExtensions.Internal
{
    public class MessagingTopicResolver
    {
        private readonly IOptions<EventStoreOptions> _options;

        public MessagingTopicResolver(IOptions<EventStoreOptions> options)
        {
            _options = options;
        }

        public string ResolveTopicName()
        {
            var topicSuffix = _options.Value.TopicSufix;
            var topic = "ch.eventStore." + topicSuffix;

            return topic;
        }
    }
}