using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Application.DataContracts;
using NBB.Contracts.ReadModel;
using NBB.Data.Abstractions.Linq;

namespace NBB.Contracts.Application.Queries
{
    public class GetContractById
    {
        public class Query : Query<ContractReadModel>
        {
            public Guid Id { get; set; }

            public Query()
                : base(null)
            {
            }
        }

        public class QueryHandler : IRequestHandler<Query, ContractReadModel>
        {
            private readonly IQueryable<ContractReadModel> _contractReadModelQuery;
            private readonly IQueryable<ContractLineReadModel> _contractLineQuery;

            public QueryHandler(IQueryable<ContractReadModel> contractReadModelQuery, IQueryable<ContractLineReadModel> contractLineQuery)
            {
                _contractReadModelQuery = contractReadModelQuery;
                _contractLineQuery = contractLineQuery;
            }


            public async Task<ContractReadModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var contract = await _contractReadModelQuery
                    //.Include(x=> x.ContractLines)
                    .Select(x=> x)
                    .SingleOrDefaultAsync(x => x.ContractId == request.Id, cancellationToken: cancellationToken);

                var contractLines = await _contractLineQuery
                    .Where(x => x.ContractId == request.Id)
                    .ToListAsync(cancellationToken: cancellationToken);

                contract?.ContractLines.AddRange(contractLines);

                return contract;
            }
        }
    }
}
