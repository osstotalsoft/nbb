// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public class MessagingHostConfigurationBuilder : IMessagingHostConfigurationBuilder, IMessagingHostOptionsBuilder,
        IMessagingHostPipelineBuilder
    {
        public IServiceProvider ApplicationServices { get; }
        private readonly IServiceCollection _serviceCollection;

        private IMessageTopicProvider _topicProvider;
        private IMessageTypeProvider _messageTypeProvider;

        private List<List<MessagingHostConfiguration.Subscriber>> _subscriberGroups = new();
        private List<MessagingHostConfiguration.Subscriber> _currentSubscriberGroup;

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

            if (_currentSubscriberGroup == null)
            {
                _currentSubscriberGroup = new List<MessagingHostConfiguration.Subscriber>();
                _subscriberGroups.Add(_currentSubscriberGroup);
            }

            return this;
        }

        public IMessagingHostPipelineBuilder WithOptions(Action<SubscriberOptionsBuilder> subscriberOptionsConfigurator)
        {
            var subscriberOptionsBuilder = new SubscriberOptionsBuilder();
            subscriberOptionsConfigurator.Invoke(subscriberOptionsBuilder);

            var options = subscriberOptionsBuilder.Build();

            foreach (var messageType in _messageTypeProvider.GetTypes())
            {
                RegisterSubscriber(messageType, options);
            }

            foreach (var topic in _topicProvider.GetTopics())
            {
                RegisterSubscriber(typeof(object), options with { TopicName = topic });
            }

            return this;
        }

        public void UsePipeline(Action<Type, IPipelineBuilder<MessagingContext>> configurePipeline)
        {
            foreach (var subscriber in _currentSubscriberGroup)
            {
                var messageType = subscriber.MessageType;
                var builder = new PipelineBuilder<MessagingContext>();
                configurePipeline?.Invoke(messageType, builder);
                subscriber.Pipeline = builder.Pipeline;
            }
            _currentSubscriberGroup = null;
        }

        private void RegisterSubscriber(Type messageType, MessagingSubscriberOptions options)
            => _currentSubscriberGroup.Add(new MessagingHostConfiguration.Subscriber { MessageType = messageType, Options = options });

        internal MessagingHostConfiguration Build()
        {
            var hostConfiguration = new MessagingHostConfiguration();

            foreach (var subscriberGroup in _subscriberGroups)
            {
                if (!subscriberGroup.Any())
                {
                    throw new Exception(
                        "No subscribers were configured for group. Use AddSubscriberServices(...).WithOptions(...) to configure subscribers.");
                }

                if (subscriberGroup.Any(x => x.Pipeline == null))
                {
                    throw new Exception(
                        "No pipeline was configured for subscribers. Add .UsePipeline(...) to configure a pipeline.");
                }

                hostConfiguration.Subscribers.AddRange(subscriberGroup);
            }

            return hostConfiguration;
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
    public interface IMessagingHostPipelineBuilder : IMessagingHostConfigurationBuilder
    {
        /// <summary>
        /// Adds the message processing pipeline to the messaging host.
        /// </summary>
        /// <param name="configurePipeline">The pipeline configurator is used to add the middleware to the pipeline.</param>
        /// <returns>The messaging host subscriberBuilder to further subscriberBuilder the messaging host. It is used in the fluent API</returns>
        void UsePipeline(Action<Type, IPipelineBuilder<MessagingContext>> configurePipeline);

        public void UsePipeline(Action<IPipelineBuilder<MessagingContext>> configurePipeline)
            => UsePipeline((_, b) => configurePipeline(b));
    }
}
