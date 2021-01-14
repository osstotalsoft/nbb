using System;
using MediatR;

namespace NBB.Payments.Application.Commands
{
    public record PayPayable(
        Guid PayableId
    ) : IRequest;
    //string IKeyProvider.Key => PayableId.ToString();
}
