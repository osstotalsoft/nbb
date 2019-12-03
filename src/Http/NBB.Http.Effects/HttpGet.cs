using System.Net.Http;
using System.Threading.Tasks;
using NBB.Core.Effects;

namespace NBB.Http.Effects
{
    public class HttpGet
    {
        public class SideEffect : ISideEffect<HttpResponseMessage>
        {
            public string Url { get; }

            public SideEffect(string url)
            {
                Url = url;
            }
        }

        internal class Handler : ISideEffectHandler<SideEffect, HttpResponseMessage>
        {
            private readonly IHttpClientFactory _httpClientFactory;

            public Handler(IHttpClientFactory httpClientFactory)
            {
                _httpClientFactory = httpClientFactory;
            }
            public Task<HttpResponseMessage> Handle(SideEffect sideEffect)
            {
                var httpClient = _httpClientFactory.CreateClient();
                return httpClient.GetAsync(sideEffect.Url);
            }
        }
    }
}
