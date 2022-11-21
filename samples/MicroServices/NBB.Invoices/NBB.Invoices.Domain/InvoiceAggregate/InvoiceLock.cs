// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Domain;
using System;

namespace NBB.Invoices.Domain.InvoiceAggregate
{
    public class InvoiceLock : EventSourcedAggregateRoot<Guid>
    {
        public record LockCreated(Guid InvoiceId, int LockTimeoutMs);
        public record LockAcquired(Guid InvoiceId, DateTime AcquireDate);
        public record LockRealeased(Guid InvoiceId, DateTime ReleaseDate);

        public class InvoiceLockAlreadyAcquiredException : Exception
        {
            public Guid InvoiceId { get; }
            public InvoiceLockAlreadyAcquiredException(Guid invoiceId)
                : base("Invoice already locked")
            {
                InvoiceId = invoiceId;
            }
        }

        public Guid InvoicetId { get; private set; }
        public int LockTimeoutMs { get; private set; }
        public bool IsLocked { get; private set; }
        public DateTime LockAcquiredAt { get; private set; }

        //needed 4 repository should be private
        public InvoiceLock()
        {
        }

        public InvoiceLock(Guid invoiceId, int lockTimeoutMs)
            => Emit(new LockCreated(invoiceId, lockTimeoutMs));

        public void Acquire()
        {
            var now = DateTime.Now;

            if (IsLocked && (now - LockAcquiredAt).TotalMilliseconds < LockTimeoutMs)
            {
                throw new InvoiceLockAlreadyAcquiredException(InvoicetId);
            }

            Emit(new LockAcquired(this.InvoicetId, now));
        }

        public void Release()
            => Emit(new LockRealeased(this.InvoicetId, DateTime.Now));


        private void Apply(LockCreated e)
            => (InvoicetId, LockTimeoutMs) = (e.InvoiceId, e.LockTimeoutMs);

        private void Apply(LockAcquired e)
        {
            IsLocked = true;
            LockAcquiredAt = e.AcquireDate;
        }

        private void Apply(LockRealeased e)
        {
            IsLocked = false;
            LockAcquiredAt = DateTime.MinValue;
        }

        public override Guid GetIdentityValue() => InvoicetId;
    }
}
