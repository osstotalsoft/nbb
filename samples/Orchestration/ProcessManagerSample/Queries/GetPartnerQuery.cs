using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Application.DataContracts;

namespace ProcessManagerSample.Queries
{
    public class GetPartnerQuery : Query<Partner>
    {
      
    }

    public class GetPartnerQueryHandler : IRequestHandler<GetPartnerQuery, Partner>
    {
        public async Task<Partner> Handle(GetPartnerQuery request, CancellationToken cancellationToken)
        {
            return new Partner();
        }
    }

    public class Partner
    {
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
    }
}
