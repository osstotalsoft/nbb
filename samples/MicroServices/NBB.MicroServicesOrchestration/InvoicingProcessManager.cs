using MediatR;
using NBB.Contracts.PublishedLanguage;
using NBB.Invoices.PublishedLanguage;
using NBB.Payments.PublishedLanguage;
using NBB.ProcessManager.Definition.Builder;
using System;

namespace MicroServicesOrchestration
{
    public enum InvoicingStatus
    {
        AwaitingInvoice,
        AwaitingPayable,
        AwaitingPayment,
        PaymentReceived,
        SuccessfullyCompleted,
        PaymentFailure
    }

    public record InvoicingState
    {
        public InvoicingStatus Status { get; init; }
        public Guid ContractId { get; init; }
    }

    public record PayableExpired(Guid PayableId, Guid ContractId) : INotification;

    public class InvoicingProcessManager : AbstractDefinition<InvoicingState>
    {
        public InvoicingProcessManager()
        {
            Event<ContractValidated>(builder => builder.CorrelateById(contract => contract.ContractId));
            Event<InvoiceCreated>(builder => builder.CorrelateById(invoice => invoice.ContractId.Value));
            Event<PayableCreated>(builder => builder.CorrelateById(payable => payable.ContractId.Value));
            Event<PaymentReceived>(builder => builder.CorrelateById(payment => payment.ContractId.Value));
            Event<PayableExpired>(builder => builder.CorrelateById(ev => ev.ContractId));
            Event<InvoiceMarkedAsPayed>(builder => builder.CorrelateById(ev => ev.ContractId.Value));

            StartWith<ContractValidated>()
                .SendCommand((ev, state) => new CreateInvoice(ev.Amount, ev.ClientId, ev.ContractId))
                .SetState((ev, state) => state.Data with { Status = InvoicingStatus.AwaitingInvoice, ContractId = ev.ContractId });

            When<InvoiceCreated>((ev, state) => ev.ContractId.HasValue)
                .SendCommand((ev, state) => new CreatePayable(ev.ClientId, ev.Amount, ev.InvoiceId, ev.ContractId))
                .SetState((ev, state) => state.Data with { Status = InvoicingStatus.AwaitingPayable });

            When<PayableCreated>((ev, state) => ev.ContractId.HasValue)
                .RequestTimeout(TimeSpan.FromDays(1), (ev, data) => new PayableExpired(ev.PayableId, ev.ContractId.Value))
                .SetState((ev, state) => state.Data with { Status = InvoicingStatus.AwaitingPayment });

            When<PayableExpired>((ev, state) => state.Data.Status == InvoicingStatus.AwaitingPayment)
                .SetState((ev, state) => state.Data with { Status = InvoicingStatus.PaymentFailure })
                .Complete();

            When<PaymentReceived>((ev, state) => ev.ContractId.HasValue && ev.InvoiceId.HasValue)
                .SendCommand((ev, state) => new MarkInvoiceAsPayed(ev.InvoiceId.Value, ev.PaymentId))
                .SetState((ev, state) => state.Data with { Status = InvoicingStatus.PaymentReceived });

            When<InvoiceMarkedAsPayed>()
                .SetState((ev, state) => state.Data with { Status = InvoicingStatus.SuccessfullyCompleted })
                .Complete();
        }
    }
}
