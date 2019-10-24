namespace NBB.ProcessManager.Definition.Effects
{
    public class SendCommand : Effect
    {
        public object Command { get; }

        public SendCommand(object command)
        {
            Command = command;
        }
    }
}