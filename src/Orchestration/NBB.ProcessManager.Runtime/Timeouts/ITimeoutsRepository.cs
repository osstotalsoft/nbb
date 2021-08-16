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
        /// Removes the timeout if it hasn't been previously removed.
        /// </summary>
        /// <param name="timeoutId">The timeout id to remove.</param>
        /// <returns><c>true</c> when the timeout has successfully been removed by this method call, <c>false</c> otherwise.</returns>
        Task<bool> TryRemove(Guid timeoutId);

        /// <summary>
        /// Returns the timeout with the given id from the storage. The timeout will remain in the storage.
        /// </summary>
        /// <param name="timeoutId">The id of the timeout to fetch.</param>
        /// <returns><see cref="Timeout" /> with the given id if present in the storage or <c>null</c> otherwise.</returns>
        Task<TimeoutRecord> Peek(Guid timeoutId);

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