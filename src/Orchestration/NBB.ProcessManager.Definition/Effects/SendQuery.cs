namespace NBB.ProcessManager.Definition.Effects
{
    public class SendQuery<T> : IEffect<T>
    {
        public object Query { get; }

        public SendQuery(object query)
        {
            Query = query;
        }
    }
}