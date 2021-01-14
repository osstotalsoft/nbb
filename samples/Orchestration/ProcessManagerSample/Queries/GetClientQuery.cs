using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ProcessManagerSample.Queries
{
    public record GetClientQuery : IRequest<Client>;

    public class GetClientQueryHandler : IRequestHandler<GetClientQuery, Client>
    {
        public Task<Client> Handle(GetClientQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Client("ion", "vasile"));
        }
    }

    public record Client(
        string ClientName,
        string ClientCode
    );
}
