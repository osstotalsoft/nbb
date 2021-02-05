using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Host.Builder
{
    public class MessagingHostBuilder : IMessagingHostBuilder, IMessagingHostOptionsBuilder,
        IMessagingHostPipelineBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private IMessageTopicProvider _topicProvider;
        private IMessageTypeProvider _messageTypeProvider;
        private MessagingHostConfiguration _hostConfiguration = new();
        private MessagingHostConfiguration.SubscriberGroup _currentSubscriberGroup;

        public MessagingHostBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }


        public IMessagingHostOptionsBuilder AddSubscriberServices(Action<ITypeSourceSelector> builder)
        {
            var subscriberServiceSelector = new TypeSourceSelector(_serviceCollection);
            builder?.Invoke(subscriberServiceSelector);

            _messageTypeProvider = subscriberServiceSelector;
            _topicProvider = subscriberServiceSelector;

            _currentSubscriberGroup = new();
            _hostConfiguration.SubscriberGroups.Add(_currentSubscriberGroup);

            return this;
        }

        public IMessagingHostPipelineBuilder WithOptions(Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator)
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

            return this;
        }

        public void UsePipeline(Action<IPipelineBuilder<MessagingEnvelope>> configurePipeline)
        {
            _currentSubscriberGroup.PipelineConfigurator = configurePipeline;
        }

        private void RegisterHostedService(Type serviceType,
            Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator,
            string topicName = null)
        {
            var subscriberOptionsBuilder = new SubscriberOptionsBuilder();
            subscriberOptionsConfigurator.Invoke(subscriberOptionsBuilder);

            var options = subscriberOptionsBuilder.Build();

            if (topicName != null)
            {
                options = options with {TopicName = topicName};
            }

            _currentSubscriberGroup.Subscribers.Add((serviceType, options));
        }

        internal void Build()
        {
            foreach (var subscriberGroup in _hostConfiguration.SubscriberGroups)
            {
                if (!subscriberGroup.Subscribers.Any())
                {
                    throw new Exception(
                        "No subscribers were configured. Use AddSubscribers(...).AddOptions(...) to configure subscribers.");
                }

                if (subscriberGroup.PipelineConfigurator == null)
                {
                    throw new Exception("No pipeline was configured. Call UsePipeline(...) to configure a pipeline");
                }

                foreach (var (type, options) in subscriberGroup.Subscribers)
                {
                    _serviceCollection.AddSingleton(sp =>
                        (IHostedService) ActivatorUtilities.CreateInstance(sp, type, options, subscriberGroup.PipelineConfigurator));
                }    
            }
            
        }
    }

    public interface IMessagingHostBuilder
    {
        /// <summary>
        /// Adds the subscriber services to the messaging host.
        /// The subscriber services are background services (hosted services) that consume messages from the bus.
        /// </summary>
        /// <param name="builder">The builder is used to configure the message types or topics for which subscriber services are added.</param>
        /// <returns>The options builder</returns>
        IMessagingHostOptionsBuilder AddSubscriberServices(Action<ITypeSourceSelector> builder);
    }


    /// <summary>
    /// Used to configure the messaging host subscriber options
    /// </summary>
    public interface IMessagingHostOptionsBuilder
    {
        /// <summary>
        /// Specify subscriber options for the previously added message subscribers.
        /// </summary>
        /// <param name="subscriberOptionsConfigurator">The subscriber options builder is used to configure the options.</param>
        /// <returns></returns>
        IMessagingHostPipelineBuilder WithOptions(
            Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator);

        /// <summary>
        /// Specify default subscriber options for the previously added message subscribers.
        /// </summary>
        /// <returns></returns>
        IMessagingHostPipelineBuilder WithDefaultOptions() => WithOptions(_ => { });
    }

    /// <summary>
    /// Used to configure the messaging host pipeline
    /// </summary>
    public interface IMessagingHostPipelineBuilder
    {
        /// <summary>
        /// Adds the message processing pipeline to the messaging host.
        /// </summary>
        /// <param name="configurePipeline">The pipeline configurator is used to add the middleware to the pipeline.</param>
        /// <returns>The messaging host builder to further configure the messaging host. It is used in the fluent API</returns>
        void UsePipeline(Action<IPipelineBuilder<MessagingEnvelope>> configurePipeline);
    }
}