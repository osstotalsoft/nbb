using System;
using NBB.Domain;
using NBB.Payments.Domain.PayableAggregate.DomainEvents;

namespace NBB.Payments.Domain.PayableAggregate
{
    public class Payable : EmitApplyAggregateRoot<Guid>
    {
        public Guid PayableId { get; private set; }
        public Guid ClientId { get; private set; }
        public decimal Amount { get; private set; }
        public Guid? InvoiceId { get; private set; }
        public Payment Payment { get; private set; }
        public bool IsPayed => Payment != null;

        //needed 4 repository should be private
        public Payable()
        {
        }

        public Payable(Guid clientId, decimal amount, Guid? invoiceId)
        {
            Emit(new PayableCreated(Guid.NewGuid(), invoiceId, clientId, amount));
        }

        public override Guid GetIdentityValue() => PayableId;

        public void Pay()
        {
            if (this.IsPayed)
                throw new Exception("payment already payed");

            Emit(new PaymentReceived(Guid.NewGuid(), this.PayableId, this.InvoiceId, DateTime.Now));
        }

        private void Apply(PayableCreated e)
        {
            this.PayableId = e.PayableId;
            this.ClientId = e.ClientId;
            this.Amount = e.Amount;
            this.InvoiceId = e.InvoiceId;
        }

        private void Apply(PaymentReceived e)
        {
            this.Payment = new Payment(e.PaymentId, e.PaymentDate, this.PayableId);
        }

    }
}
