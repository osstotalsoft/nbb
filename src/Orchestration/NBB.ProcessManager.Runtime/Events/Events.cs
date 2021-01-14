namespace NBB.ProcessManager.Runtime.Events
{
    public record EventReceived<TEvent>(TEvent ReceivedEvent);

    public record ProcessAborted;

    public record ProcessCompleted<TEvent>(TEvent ReceivedEvent);

    public record ProcessStarted(object InstanceId);

    public record ProcessTimeout;
}