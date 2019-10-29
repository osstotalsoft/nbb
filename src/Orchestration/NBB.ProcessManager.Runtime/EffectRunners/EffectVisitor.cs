using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Definition.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Runtime.Timeouts;

namespace NBB.ProcessManager.Runtime.EffectRunners
{

    public class EffectVisitor : IEffectVisitor
    {
        private readonly IServiceProvider _serviceProvider;

        public EffectVisitor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResult> Visit<TResult>(HttpEffect<TResult> effect)
        {
            throw new NotImplementedException();
        }


        public async Task<Unit> Visit(RequestTimeoutEffect effect)
        {
            var timeoutsManager = _serviceProvider.GetRequiredService<TimeoutsManager>();
            var timeoutsRepository = _serviceProvider.GetRequiredService<ITimeoutsRepository>();
            var currentTimeProvider = _serviceProvider.GetRequiredService<Func<DateTime>>();

            var dueDate = currentTimeProvider().Add(effect.TimeSpan);
            await timeoutsRepository.Add(new TimeoutRecord(effect.InstanceId, dueDate, effect.Message, effect.MessageType));
            timeoutsManager.NewTimeoutRegistered(dueDate);

            return Unit.Value;
        }

        public async Task<Unit> Visit(CancelTimeoutsEffect effect)
        {
            var timeoutsRepository = _serviceProvider.GetRequiredService<ITimeoutsRepository>();
            await timeoutsRepository.RemoveTimeoutBy(effect.InstanceId?.ToString());

            return Unit.Value;
        }

        public Task<TResult[]> Visit<TResult>(ParallelEffect<TResult> effect)
        {
            var list = effect.Effects.Select(ef => ef.Accept(this)).ToList();
            return Task.WhenAll(list);
        }

        public async Task<Unit> Visit(SequentialEffect effect)
        {
            await effect.Effect1.Accept(this);
            await effect.Effect2.Accept(this);

            return Unit.Value;
        }

        public Task<Unit> Visit(NoEffect effect)
        {
            return Unit.Task;
        }

        public async Task<Unit> Visit(PublishMessageEffect effect)
        {
            var messageBusPublisher = _serviceProvider.GetRequiredService<IMessageBusPublisher>();
            await messageBusPublisher.PublishAsync(effect.Message);
            return Unit.Value;
        }

        public Task<TResult> Visit<TResult>(SendQueryEffect<TResult> effect)
        {
            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            return mediator.Send(effect.Query);
        }

        public async Task<TEffectResult2> Visit<TEffectResult1, TEffectResult2>(BoundedEffect<TEffectResult1, TEffectResult2> effect)
        {
            var r1 = await effect.Effect.Accept(this);
            return await effect.Continuation(r1).Accept(this);
        }
    }
}