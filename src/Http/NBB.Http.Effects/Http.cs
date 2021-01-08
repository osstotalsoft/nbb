using System.Net.Http;
using NBB.Core.Effects;

namespace NBB.Http.Effects
{
    public static class Http
    {
        public static Effect<HttpResponseMessage> Get(string url) => Effect.Of<HttpGet.SideEffect, HttpResponseMessage>(new HttpGet.SideEffect(url));
    }
}
