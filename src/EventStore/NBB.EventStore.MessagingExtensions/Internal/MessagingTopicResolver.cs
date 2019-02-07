using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.EventStore.MessagingExtensions.Internal
{
    public class MessagingTopicResolver
    {
        private readonly IConfiguration _configuration;

        public MessagingTopicResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ResolveTopicName()
        {
            var topicSuffix = _configuration["EventStore:NBB:TopicSufix"];
            var topic = "ch.eventStore." + topicSuffix;

            return topic;
        }
    }
}
