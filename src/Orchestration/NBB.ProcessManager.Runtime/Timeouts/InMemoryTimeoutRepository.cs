// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public class InMemoryTimeoutRepository : ITimeoutsRepository
    {
        private readonly Func<DateTime> _currentTimeProvider;
        private readonly ReaderWriterLockSlim _readerWriterLock = new();
        private readonly List<TimeoutRecord> _storage = new();
        public static readonly TimeSpan EmptyResultsNextTimeToRunQuerySpan = TimeSpan.FromMinutes(1);

        public InMemoryTimeoutRepository(Func<DateTime> currentTimeProvider)
        {
            _currentTimeProvider = currentTimeProvider;
        }


        public Task Add(TimeoutRecord timeout)
        {
            try
            {
                _readerWriterLock.EnterWriteLock();
                _storage.Add(timeout);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        public Task RemoveTimeoutBy(string instanceId)
        {
            try
            {
                _readerWriterLock.EnterWriteLock();
                for (var index = 0; index < _storage.Count;)
                {
                    var timeoutData = _storage[index];
                    if (timeoutData.ProcessManagerInstanceId == instanceId)
                    {
                        _storage.RemoveAt(index);
                        continue;
                    }
                    index++;
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        public Task<TimeoutBatch> GetNextBatch(DateTime startSlice)
        {
            var now = _currentTimeProvider();
            var nextTimeToRunQuery = DateTime.MaxValue;
            var dueTimeouts = new List<TimeoutRecord>();

            try
            {
                _readerWriterLock.EnterReadLock();

                foreach (var data in _storage)
                {
                    if (data.DueDate > now && data.DueDate < nextTimeToRunQuery)
                    {
                        nextTimeToRunQuery = data.DueDate;
                    }
                    if (data.DueDate > startSlice && data.DueDate <= now)
                    {
                        dueTimeouts.Add(data);
                    }
                }
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            if (nextTimeToRunQuery == DateTime.MaxValue)
            {
                nextTimeToRunQuery = now.Add(EmptyResultsNextTimeToRunQuerySpan);
            }

            return Task.FromResult(new TimeoutBatch(dueTimeouts.ToArray(), nextTimeToRunQuery));
        }
         
    }
}
