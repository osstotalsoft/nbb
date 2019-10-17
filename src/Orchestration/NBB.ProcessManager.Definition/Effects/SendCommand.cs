namespace NBB.ProcessManager.Definition.Effects
{
    public class SendCommand : IEffect
    {
        public object Command { get; }

        public SendCommand(object command)
        {
            Command = command;
        }
    }
}