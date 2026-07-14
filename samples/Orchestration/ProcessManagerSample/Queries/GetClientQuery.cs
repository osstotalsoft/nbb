// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Mediator;

namespace ProcessManagerSample.Queries
{
    public record GetClientQuery : IRequest<Client>;

    public class GetClientQueryHandler : IRequestHandler<GetClientQuery, Client>
    {
        public ValueTask<Client> Handle(GetClientQuery request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new Client("ion", "vasile"));
        }
    }

    public record Client(string ClientName, string ClientCode);
}