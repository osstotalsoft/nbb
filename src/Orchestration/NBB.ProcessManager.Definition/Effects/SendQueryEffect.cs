namespace NBB.ProcessManager.Definition.Effects
{
    public class SendQueryEffect<T> : IEffect<T>
    {
        public object Query { get; }

        public SendQueryEffect(object query)
        {
            Query = query;
        }
    }
}