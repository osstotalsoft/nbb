using System;

namespace NBB.ProcessManager.Definition
{
    public class InstanceData<TData>
        where TData : struct
    {
        //TO REMOVE CorrelationId setter
        public object CorrelationId { get; set; }
        public TData Data { get; set; }
    }
}