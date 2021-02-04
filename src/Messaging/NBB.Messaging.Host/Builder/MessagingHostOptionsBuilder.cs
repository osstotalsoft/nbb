using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NBB.Messaging.Host.Builder
{
    /// <summary>
    /// Used to configure the messaging host subscriber options
    /// </summary>
    public class MessagingHostOptionsBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IMessageTopicProvider _topicProvider;
        private readonly IMessageTypeProvider _messageTypeProvider;

        public MessagingHostOptionsBuilder(IServiceCollection serviceCollection,
            IMessageTypeProvider messageTypeProvider, IMessageTopicProvider topicProvider)
        {
            _serviceCollection = serviceCollection;
            _messageTypeProvider = messageTypeProvider;
            _topicProvider = topicProvider;
        }

        /// <summary>
        /// Specify default subscriber options for the previously added message subscribers.
        /// </summary>
        /// <returns></returns>
        public MessagingHostPipelineBuilder WithDefaultOptions()
        {
            return WithOptions(_ => { });
        }

        /// <summary>
        /// Specify subscriber options for the previously added message subscribers.
        /// </summary>
        /// <param name="subscriberOptionsConfigurator">The subscriber options builder is used to configure the options.</param>
        /// <returns></returns>
        public MessagingHostPipelineBuilder WithOptions(
            Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator)
        {
            foreach (var messageType in _messageTypeProvider.GetTypes())
            {
                RegisterHostedService(typeof(MessageBusSubscriberService<>).MakeGenericType(messageType),
                    subscriberOptionsConfigurator);
            }

            foreach (var topic in _topicProvider.GetTopics())
            {
                RegisterHostedService(typeof(MessageBusSubscriberService), subscriberOptionsConfigurator, topic);
            }

            return new MessagingHostPipelineBuilder(_serviceCollection);
        }

        private void RegisterHostedService(Type serviceType,
            Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator,
            string topicName = null)
        {
            _serviceCollection.AddSingleton(sp =>
            {
                var subscriberOptionsBuilder = new SubscriberOptionsBuilder();
                subscriberOptionsConfigurator.Invoke(subscriberOptionsBuilder);

                var options = subscriberOptionsBuilder.Build();

                if (topicName != null)
                {
                    options = options with {TopicName = topicName};
                }

                return (IHostedService) ActivatorUtilities.CreateInstance(sp, serviceType, options);
            });
        }

        public class SubscriberOptionsBuilder
        {
            private MessagingSubscriberOptions _subscriberOptions = new();

            public SubscriberOptionsBuilder ConfigureTransport(
                Func<SubscriptionTransportOptions, SubscriptionTransportOptions> subscriberOptionsConfigurator)
            {
                _subscriberOptions = _subscriberOptions with
                {
                    Transport = subscriberOptionsConfigurator(_subscriberOptions.Transport)
                };

                return this;
            }

            public SubscriberOptionsBuilder UseDynamicDeserialization(
                IEnumerable<Assembly> dynamicDeserializationScannedAssemblies)
            {
                _subscriberOptions = _subscriberOptions with
                {
                    SerDes = _subscriberOptions.SerDes with
                    {
                        UseDynamicDeserialization = true,
                        DynamicDeserializationScannedAssemblies = dynamicDeserializationScannedAssemblies
                    }
                };

                return this;
            }

            internal MessagingSubscriberOptions Build()
                => _subscriberOptions;
        }
    }
}