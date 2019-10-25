using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Definition.Effects;
using NBB.ProcessManager.Runtime.Timeouts;
using System;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Runtime.EffectRunners
{
    public class EffectRunners
    {
        public static Func<IServiceProvider, EffectRunner> PublishMessageEffectRunner()
        {
            return serviceProvider =>
            {
                var messageBusPublisher = serviceProvider.GetRequiredService<IMessageBusPublisher>();
                return effect =>
                {
                    if (effect is PublishMessageEffect publishMessage)
                        return messageBusPublisher.PublishAsync(publishMessage.Message);
                    return Task.CompletedTask;
                };
            };
        }

        public static Func<IServiceProvider, EffectRunner> RequestTimeoutEffectRunner()
        {
            return serviceProvider =>
            {
                var timeoutsManager = serviceProvider.GetRequiredService<TimeoutsManager>();
                var timeoutsRepository = serviceProvider.GetRequiredService<ITimeoutsRepository>();
                var currentTimeProvider = serviceProvider.GetRequiredService<Func<DateTime>>();

                return async effect =>
                {
                    if (effect is RequestTimeoutEffect requestTimeout)
                    {
                        var dueDate = currentTimeProvider().Add(requestTimeout.TimeSpan);
                        await timeoutsRepository.Add(new TimeoutRecord(requestTimeout.InstanceId, dueDate, requestTimeout.Message, requestTimeout.MessageType));
                        timeoutsManager.NewTimeoutRegistered(dueDate);
                    }
                };
            };
        }

        public static Func<IServiceProvider, EffectRunner> CancelTimeoutsEffectRunner()
        {
            return serviceProvider =>
            {
                var timeoutsRepository = serviceProvider.GetRequiredService<ITimeoutsRepository>();
                return async effect =>
                {
                    if (effect is CancelTimeoutsEffect cancelTimeouts)
                    {
                        await timeoutsRepository.RemoveTimeoutBy(cancelTimeouts.InstanceId?.ToString());
                    }
                };
            };
        }

        public static Func<IServiceProvider, EffectRunner> SequentialEffectRunner()
        {
            return serviceProvider =>
            {
                var effectRunnerFactory = serviceProvider.GetRequiredService<EffectRunnerFactory>();
                return async effect =>
                {
                    if (effect is SequentialEffect sequentialEffect)
                    {
                        await effectRunnerFactory(sequentialEffect.Effect1.GetType())(sequentialEffect.Effect1);
                        await effectRunnerFactory(sequentialEffect.Effect2.GetType())(sequentialEffect.Effect2);
                    }
                };
            };
        }

        public static Func<IServiceProvider, EffectRunner> BoundedEffectRunner<TEffectResult1, TEffectResult2>()
        {
            return serviceProvider =>
            {
                var effectRunnerFactory1 = serviceProvider.GetRequiredService<EffectRunnerFactory<TEffectResult1>>();
                var effectRunnerFactory2 = serviceProvider.GetRequiredService<EffectRunnerFactory<TEffectResult2>>();
                return async effect =>
                {
                    if (effect is BoundedEffect<TEffectResult1, TEffectResult2> boundedEffect)
                    {
                        var result = await effectRunnerFactory1(boundedEffect.Effect.GetType())(boundedEffect.Effect);
                        var effect2 = boundedEffect.Continuation(result);
                        await effectRunnerFactory2(effect2.GetType())(effect2);
                    }
                };
            };
        }

        public static Func<IServiceProvider, EffectRunner<TResult>> SendQueryEffectRunner<TResult>()
        {
            return serviceProvider =>
            {
                var mediator = serviceProvider.GetRequiredService<IMediator>();
                return async effect =>
                {
                    if (effect is SendQueryEffect<TResult> sendQueryEffect)
                    {
                        return await mediator.Send(sendQueryEffect.Query);
                    }

                    return default(TResult);
                };
            };
        }


        public static Func<IServiceProvider, EffectRunner> NoOpEffect()
        {
            return serviceProvider => { return effect => Task.CompletedTask; };
        }
    }
}