// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ProcessManagerSample.Queries
{
    public record GetPartnerQuery : IRequest<Partner>;

    public class GetPartnerQueryHandler : IRequestHandler<GetPartnerQuery, Partner>
    {
        public Task<Partner> Handle(GetPartnerQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Partner("ion","vasile"));
        }
    }

    public record Partner(string PartnerName, string PartnerCode);
}
