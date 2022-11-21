// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public interface ITimeoutsRepository
    {
        /// <summary>
        /// Adds a new timeout.
        /// </summary>
        /// <param name="timeout">Timeout data.</param>
        Task Add(TimeoutRecord timeout);

        /// <summary>
        /// Removes the timeouts by instance id.
        /// </summary>
        /// <param name="instanceId">The instance id of the timeouts to remove.</param>
        Task RemoveTimeoutBy(string instanceId);

        /// <summary>
        /// Retrieves the next range of timeouts that are due.
        /// </summary>
        /// <param name="startSlice">The time where to start retrieving the next slice, the slice should exclude this date.</param>
        /// <returns>Returns the next range of timeouts that are due.</returns>
        Task<TimeoutBatch> GetNextBatch(DateTime startSlice);
    }
}
