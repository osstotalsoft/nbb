namespace NBB.ProcessManager.Runtime.Events
{
    public record EventReceived<TEvent>(
        TEvent ReceivedEvent
    );
}