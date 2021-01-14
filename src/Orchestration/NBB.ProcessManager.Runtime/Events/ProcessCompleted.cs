namespace NBB.ProcessManager.Runtime.Events
{
    public record ProcessCompleted<TEvent>(
        TEvent ReceivedEvent
    );
}