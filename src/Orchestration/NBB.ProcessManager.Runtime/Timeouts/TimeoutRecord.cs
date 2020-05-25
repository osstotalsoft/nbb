using System;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public class TimeoutRecord
    {
        /// <summary>
        /// Id of this timeout.
        /// </summary>
        public Guid Id { get; }

        public object Message { get; }
        public Type MessageType { get; }

        /// <summary>
        /// The ProcessManagerInstanceId who requested the timeout.
        /// </summary>
        public string ProcessManagerInstanceId { get; }

        /// <summary>
        /// The time at which the timeout expires.
        /// </summary>
        public DateTime DueDate { get; }

        public TimeoutRecord(string processManagerInstanceId, DateTime dueDate, object message, Type messageType, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            ProcessManagerInstanceId = processManagerInstanceId;
            DueDate = dueDate;
            Message = message;
            MessageType = messageType;
        }

        public override string ToString()
        {
            return $"Timeout({Id}) - Expires:{DueDate}, InstanceId:{ProcessManagerInstanceId}";
        }
    }
}