using System.Net.Http;
using NBB.Core.Effects;

namespace NBB.Http.Effects
{
    public static class Http
    {
        public static Effect<HttpResponseMessage> Get(string url) => Effect.Of(new HttpGet.SideEffect(url));
    }
}
