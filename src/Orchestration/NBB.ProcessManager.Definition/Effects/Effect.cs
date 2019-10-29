using MediatR;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Definition.Effects
{
    public class Effect<TResult> : IEffect<TResult>
    {
        public Func<IEffectRunner, Task<TResult>> Computation { get; }

        public Effect(Func<IEffectRunner, Task<TResult>> computation)
        {
            Computation = computation;
        }
    }

    public class EffectsFactory
    {
        public static IEffect<TResult> Http<TResult>(HttpRequestMessage message)
        {
            return new Effect<TResult>(runner => runner.Http<TResult>(message));
        }

        public static IEffect<TResult> Query<TResult>(IRequest<TResult> query)
        {
            return new Effect<TResult>(runner => runner.SendQuery(query));
        }

        public static IEffect<Unit> PublishMessage(object message)
        {
            return new Effect<Unit>(async runner =>
            {
                await runner.PublishMessage(message);
                return Unit.Value;
            });
        }

        public static IEffect<TResult[]> WhenAll<TResult>(params IEffect<TResult>[] effects)
        {
            return new Effect<TResult[]>(runner =>
            {
                var list = effects.Select(effect => effect.Computation(runner));
                return Task.WhenAll(list);
            });
        }

        public static IEffect<Unit> RequestTimeout(string instanceId, TimeSpan timeSpan, object message, Type messageType)
        {
            return new Effect<Unit>(async runner =>
            {
                await runner.RequestTimeout(instanceId, timeSpan, message, messageType);
                return Unit.Value;
            });
        }

        public static IEffect<Unit> CancelTimeout(object instanceId)
        {
            return new Effect<Unit>(async runner =>
            {
                await runner.CancelTimeouts(instanceId);
                return Unit.Value;
            });
        }
    }
}