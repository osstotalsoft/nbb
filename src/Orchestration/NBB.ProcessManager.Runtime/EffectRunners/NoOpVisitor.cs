using MediatR;
using NBB.ProcessManager.Definition.Effects;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.EffectRunners
{

    public class NoOpVisitor : IEffectRunner
    {
        public Task<TResult> Http<TResult>(HttpRequestMessage request)
        {
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> SendQuery<TResult>(IRequest<TResult> query)
        {
            return Task.FromResult(default(TResult));
        }

        public Task PublishMessage(object message)
        {
            return Unit.Task;
        }

        public Task RequestTimeout(string instanceId, TimeSpan timeSpan, object message, Type messageType)
        {
            return Unit.Task;
        }

        public Task CancelTimeouts(object instanceId)
        {
            return Unit.Task;
        }
    }
}