using MediatR;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public record TimeoutOccured(
        string ProcessManagerInstanceId,
        object Message
    ) : INotification;
}