using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Application.DataContracts;
using NBB.Contracts.ReadModel;
using NBB.Data.Abstractions.Linq;

namespace NBB.Contracts.Application.Queries
{
    public class GetContracts
    {
        public class Query : Query<List<ContractReadModel>>
        {
            public Query()
                : base(null)
            {
            }
        }

        public class QueryHandler : IRequestHandler<Query, List<ContractReadModel>>
        {
            private readonly IQueryable<ContractReadModel> _contractReadModelQuery;

            public QueryHandler(IQueryable<ContractReadModel> contractReadModelQuery)
            {
                _contractReadModelQuery = contractReadModelQuery;
            }


            public async Task<List<ContractReadModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _contractReadModelQuery.ToListAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
