namespace NBB.ProcessManager.Definition.Effects
{
    public class PublishEvent : Effect
    {
        public object Event { get; }

        public PublishEvent(object @event)
        {
            Event = @event;
        }
    }
}