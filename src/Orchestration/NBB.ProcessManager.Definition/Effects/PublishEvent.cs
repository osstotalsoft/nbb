namespace NBB.ProcessManager.Definition.Effects
{
    public class PublishEvent : IEffect
    {
        public object Event { get; }

        public PublishEvent(object @event)
        {
            Event = @event;
        }
    }
}