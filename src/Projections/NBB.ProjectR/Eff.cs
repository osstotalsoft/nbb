using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public static class Eff
    {
        public static Effect<TMessage> None<TMessage>() => Effect.Pure<TMessage>(default);
        public static Effect<TMessage> OfMsg<TMessage>(TMessage msg) => Effect.Pure<TMessage>(msg);
    }
}
