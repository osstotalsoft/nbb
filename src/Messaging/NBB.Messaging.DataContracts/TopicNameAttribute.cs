using System;
using Microsoft.Extensions.Configuration;

namespace NBB.Messaging.DataContracts
{
    public class TopicNameAttribute : TopicNameResolverAttribute
    {
        private readonly string _topic;

        public TopicNameAttribute(string topic)
        {
            _topic = topic;
        }

        public override string ResolveTopicName(Type messageType, IConfiguration configuration)
        {
            return _topic;
        }
    }
}
