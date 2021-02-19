using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;
using System.Linq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Host.Builder
{
    public class MessagingHostConfigurationBuilder : IMessagingHostConfigurationBuilder, IMessagingHostOptionsBuilder,
        IMessagingHostPipelineBuilder
    {
        public IServiceProvider ApplicationServices { get; }
        private readonly IServiceCollection _serviceCollection;

        private IMessageTopicProvider _topicProvider;
        private IMessageTypeProvider _messageTypeProvider;
        private readonly MessagingHostConfiguration _hostConfiguration = new();
        private MessagingHostConfiguration.SubscriberGroup _currentSubscriberGroup;

        public MessagingHostConfigurationBuilder(IServiceProvider applicationServices, IServiceCollection serviceCollection)
        {
            ApplicationServices = applicationServices;
            _serviceCollection = serviceCollection;
        }


        public IMessagingHostOptionsBuilder AddSubscriberServices(Action<ITypeSourceSelector> subscriberBuilder)
        {
            var subscriberServiceSelector = new TypeSourceSelector(_serviceCollection);
            subscriberBuilder?.Invoke(subscriberServiceSelector);

            _messageTypeProvider = subscriberServiceSelector;
            _topicProvider = subscriberServiceSelector;

            _currentSubscriberGroup = new MessagingHostConfiguration.SubscriberGroup();
            _hostConfiguration.SubscriberGroups.Add(_currentSubscriberGroup);

            return this;
        }

        public IMessagingHostPipelineBuilder WithOptions(Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator)
        {
            foreach (var messageType in _messageTypeProvider.GetTypes())
            {
                RegisterSubscriber(messageType, subscriberOptionsConfigurator);
            }

            foreach (var topic in _topicProvider.GetTopics())
            {
                RegisterSubscriber(typeof(object), subscriberOptionsConfigurator, topic);
            }

            return this;
        }

        public void UsePipeline(Action<IPipelineBuilder<MessagingContext>> configurePipeline)
        {
            var builder = new PipelineBuilder<MessagingContext>();
            configurePipeline?.Invoke(builder);
            _currentSubscriberGroup.Pipeline = builder.Pipeline;
        }

        private void RegisterSubscriber(Type serviceType,
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

        internal MessagingHostConfiguration Build()
        {
            foreach (var subscriberGroup in _hostConfiguration.SubscriberGroups)
            {
                if (!subscriberGroup.Subscribers.Any())
                {
                    throw new Exception(
                        "No subscribers were configured. Use AddSubscriberServices(...).WithOptions(...) to subscriberBuilder subscribers.");
                }

                if (subscriberGroup.Pipeline == null)
                {
                    throw new Exception("No pipeline was configured. Call UsePipeline(...) to subscriberBuilder a pipeline");
                }
            }

            return _hostConfiguration;
        }
    }

    public interface IMessagingHostConfigurationBuilder
    {
        IServiceProvider ApplicationServices { get; }

        /// <summary>
        /// Adds the subscriber services to the messaging host.
        /// The subscriber services are background services (hosted services) that consume messages from the bus.
        /// </summary>
        /// <param name="subscriberBuilder">The subscriberBuilder is used to subscriberBuilder the message types or topics for which subscriber services are added.</param>
        /// <returns>The options subscriberBuilder</returns>
        IMessagingHostOptionsBuilder AddSubscriberServices(Action<ITypeSourceSelector> subscriberBuilder);
    }


    /// <summary>
    /// Used to subscriberBuilder the messaging host subscriber options
    /// </summary>
    public interface IMessagingHostOptionsBuilder
    {
        /// <summary>
        /// Specify subscriber options for the previously added message subscribers.
        /// </summary>
        /// <param name="subscriberOptionsConfigurator">The subscriber options subscriberBuilder is used to subscriberBuilder the options.</param>
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
    /// Used to subscriberBuilder the messaging host pipeline
    /// </summary>
    public interface IMessagingHostPipelineBuilder
    {
        /// <summary>
        /// Adds the message processing pipeline to the messaging host.
        /// </summary>
        /// <param name="configurePipeline">The pipeline configurator is used to add the middleware to the pipeline.</param>
        /// <returns>The messaging host subscriberBuilder to further subscriberBuilder the messaging host. It is used in the fluent API</returns>
        void UsePipeline(Action<IPipelineBuilder<MessagingContext>> configurePipeline);
    }
}