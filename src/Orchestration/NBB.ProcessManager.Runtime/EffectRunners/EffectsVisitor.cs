using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Definition.Effects;
using NBB.ProcessManager.Runtime.Timeouts;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.EffectRunners
{

    public class EffectsVisitor : IEffectRunner
    {
        private readonly IServiceProvider _serviceProvider;

        public EffectsVisitor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResult> Http<TResult>(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> SendQuery<TResult>(IRequest<TResult> query)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            return mediator.Send(query);
        }

        public Task PublishMessage(object message)
        {
            var messageBusPublisher = _serviceProvider.GetRequiredService<IMessageBusPublisher>();
            return messageBusPublisher.PublishAsync(message);
        }

        public Task RequestTimeout(string instanceId, TimeSpan timeSpan, object message, Type messageType)
        {
            var timeoutsManager = _serviceProvider.GetRequiredService<TimeoutsManager>();
            var timeoutsRepository = _serviceProvider.GetRequiredService<ITimeoutsRepository>();
            var currentTimeProvider = _serviceProvider.GetRequiredService<Func<DateTime>>();

            var dueDate = currentTimeProvider().Add(timeSpan);
            timeoutsManager.NewTimeoutRegistered(dueDate);
            return timeoutsRepository.Add(new TimeoutRecord(instanceId, dueDate, message, messageType));
        }

        public Task CancelTimeouts(object instanceId)
        {
            var timeoutsRepository = _serviceProvider.GetRequiredService<ITimeoutsRepository>();
            return timeoutsRepository.RemoveTimeoutBy(instanceId?.ToString());
        }
    }
}