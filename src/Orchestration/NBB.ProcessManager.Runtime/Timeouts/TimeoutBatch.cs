using System;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public class TimeoutBatch
    {
        /// <summary>
        /// Creates a new instance of the timeouts batch.
        /// </summary>
        /// <param name="dueTimeouts">timeouts that are due.</param>
        /// <param name="nextTimeToQuery">the next time to query for due timeouts again.</param>
        public TimeoutBatch(TimeoutRecord[] dueTimeouts, DateTime nextTimeToQuery)
        {
            DueTimeouts = dueTimeouts;
            NextTimeToQuery = nextTimeToQuery;
        }

        /// <summary>
        /// timeouts that are due.
        /// </summary>
        public TimeoutRecord[] DueTimeouts { get; }

        /// <summary>
        /// the next time to query for due timeouts again.
        /// </summary>
        public DateTime NextTimeToQuery { get; }


       
    }
}
