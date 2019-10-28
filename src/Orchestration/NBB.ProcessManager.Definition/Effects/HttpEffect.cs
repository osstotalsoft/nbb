using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Definition.Effects
{
    public class HttpEffect<TResult> : IEffect<TResult>
    {
        private readonly HttpMessageHandler _handler;

        public HttpEffect(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public Task<TResult> Accept(IEffectVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}