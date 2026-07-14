// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Mediator;

namespace ProcessManagerSample.Queries
{
    public record GetPartnerQuery : IRequest<Partner>;

    public class GetPartnerQueryHandler : IRequestHandler<GetPartnerQuery, Partner>
    {
        public ValueTask<Partner> Handle(GetPartnerQuery request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new Partner("ion","vasile"));
        }
    }

    public record Partner(string PartnerName, string PartnerCode);
}
