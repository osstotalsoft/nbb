using System;
using NBB.Domain;

namespace NBB.Payments.Domain.PayableAggregate
{
    public class Payment : Entity<Guid>
    {
        public Guid PaymentId { get; private set; }

        public DateTime PaymentDate { get; private set; }

        public Guid PayableId { get; private set; }

        public Payment(Guid paymentId, DateTime paymentDate, Guid payableId)
        {
            this.PaymentId = paymentId;
            this.PaymentDate = paymentDate;
            this.PayableId = payableId;
        }

        public override Guid GetIdentityValue() => PaymentId;
    }
}
