//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using NBB.Core.Pipeline;
//using NBB.Messaging.Abstractions;

//namespace NBB.Messaging.Host
//{
//    public class MessagingHost
//    {
//        private readonly IMessageBus _messageBus;
//        private readonly IServiceProvider _serviceProvider;
//        private readonly MessagingContextAccessor _messagingContextAccessor;
//        private readonly ILogger<MessagingHost> _logger;
//        private readonly ITopicRegistry _topicRegistry;
//        private readonly object _lockObj = new();
//        private bool _isStated = false;
//        private List<IDisposable> subscriptions = new();

//        public MessagingHost(
//            IMessageBus messageBus,
//            IServiceProvider serviceProvider,
//            MessagingContextAccessor messagingContextAccessor,
//            ILogger<MessagingHost> logger,
//            ITopicRegistry topicRegistry
//        )
//        {
//            _messageBus = messageBus;
//            _serviceProvider = serviceProvider;
//            _messagingContextAccessor = messagingContextAccessor;
//            _logger = logger;
//            _topicRegistry = topicRegistry;
//        }

//        public void Start(MessagingHostConfiguration configuration, CancellationToken cancellationToken = default)
//        {
//            //if (_isStated) return;

//            lock (_lockObj)
//            {
//                if (_isStated)
//                    return;

//                foreach (var subscriberGroup in configuration.SubscriberGroups)
//                {
//                    foreach (var (subscriberType, options) in subscriberGroup.Subscribers)
//                    {
//                        Task HandleMsg(MessagingEnvelope msg) => Handle(msg, cancellationToken);        
//                    }
                    
//                }

                

//                _isStated = true;
//            }
//        }

//        public void Stop()
//        {
//            //if (!_isStated) return;

//            lock (_lockObj)
//            {
//                if (!_isStated)
//                    return;

//                foreach (var subscription in subscriptions)
//                {
//                    subscription.Dispose();
//                }

//                subscriptions.Clear();

//                _isStated = false;
//            }
//        }


//        private async Task Handle(Type messageType, MessagingEnvelope message, string topicName,  CancellationToken cancellationToken)
//        {
//            using var scope = _serviceProvider.CreateScope();

//            var topicName = _topicRegistry.GetTopicForName(_subscriberOptions?.TopicName, false) ??
//                            _topicRegistry.GetTopicForMessageType(typeof(TMessage), false);

//            var pipelineBuilder = new PipelineBuilder<MessagingEnvelope>(scope.ServiceProvider);
//            _pipelineConfigurator.Invoke(pipelineBuilder);
//            var pipeline = pipelineBuilder.Pipeline;

//            _messagingContextAccessor.MessagingContext = new MessagingContext(message, topicName);

//            await pipeline(message, cancellationToken);
//        }
//    }
//}
