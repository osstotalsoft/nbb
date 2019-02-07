using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;

namespace NBB.Messaging.Host.Builder
{
    /// <summary>
    /// Used to configure the messaging host subscriber options
    /// </summary>
    public class MessagingHostOptionsBuilder
    {
        private readonly MessagingHostBuilder _hostBuilder;
        private readonly IServiceCollection _serviceCollection;
        private readonly IMessageTopicProvider _topicProvider;
        private readonly IMessageTypeProvider _messageTypeProvider;
        
        public MessagingHostOptionsBuilder(MessagingHostBuilder hostBuilder, IServiceCollection serviceCollection,
            IMessageTypeProvider messageTypeProvider, IMessageTopicProvider topicProvider)
        {
            _hostBuilder = hostBuilder;
            _serviceCollection = serviceCollection;
            _messageTypeProvider = messageTypeProvider;
            _topicProvider = topicProvider;
        }

        /// <summary>
        /// Specify default subscriber options for the previously added message subscribers.
        /// </summary>
        /// <returns></returns>
        public MessagingHostBuilder WithDefaultOptions()
        {
            return WithOptions();
        }

        /// <summary>
        /// Specify subscriber options for the previously added message subscribers.
        /// </summary>
        /// <param name="subscriberOptionsBuilder">The subscriber options builder is used to configure the options.</param>
        /// <returns></returns>
        public MessagingHostBuilder WithOptions(
            Action<MessagingSubscriberOptionsBuilder> subscriberOptionsBuilder = null)
        {
            foreach (var messageType in _messageTypeProvider.GetTypes())
            {
                RegisterHostedService(typeof(MessageBusSubscriberService<>).MakeGenericType(messageType),
                    subscriberOptionsBuilder);
            }

            foreach (var topic in _topicProvider.GetTopics())
            {
                RegisterHostedService(typeof(MessagingTopicSubscriberService), subscriberOptionsBuilder, topic);
            }

            return _hostBuilder;
        }

        private void RegisterHostedService(Type serviceType,
            Action<MessagingSubscriberOptionsBuilder> subscriberOptionsConfigurator = null, string topicName = null)
        {
            _serviceCollection.AddSingleton(sp =>
            {
                var builder = new MessagingSubscriberOptionsBuilder(sp.GetService<MessagingSubscriberOptions>());
                subscriberOptionsConfigurator?.Invoke(builder);

                return topicName == null
                    ? (IHostedService) ActivatorUtilities.CreateInstance(sp, serviceType, builder.Options)
                    : (IHostedService) ActivatorUtilities.CreateInstance(sp, serviceType, builder.Options, topicName);
            });
        }
    }
}
