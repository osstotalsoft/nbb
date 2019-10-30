using System;
using System.Net.Http;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public interface IEffectRunner
    {
        Task<TResult> Http<TResult>(HttpRequestMessage request);
        Task<TResult> SendQuery<TResult>(IRequest<TResult> query);
        Task PublishMessage(object message);
        Task RequestTimeout(string instanceId, TimeSpan timeSpan, object message, Type messageType);
        Task CancelTimeouts(object instanceId);
    }
}