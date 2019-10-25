namespace NBB.ProcessManager.Definition.Effects
{
    public class PublishMessageEffect : IEffect
    {
        public object Message { get; }

        public PublishMessageEffect(object message)
        {
            Message = message;
        }
    }
}