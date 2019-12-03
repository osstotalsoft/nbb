using NBB.Core.Effects;

namespace NBB.Messaging.Effects
{
    public static class MessageBus
    {
        public static IEffect<int> Publish(string url) => Effect.Of(new PublishMessage.SideEffect(url));
    }
}
