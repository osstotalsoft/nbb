using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Definition.Effects;
using NBB.ProcessManager.Runtime.Timeouts;
using System;
using System.Threading.Tasks;

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
                    if (effect is RequestTimeout requestTimeout)
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
                    if (effect is CancelTimeouts cancelTimeouts)
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


        public static Func<IServiceProvider, EffectRunner> NoOpEffect()
        {
            return serviceProvider => { return effect => Task.CompletedTask; };
        }
    }
}