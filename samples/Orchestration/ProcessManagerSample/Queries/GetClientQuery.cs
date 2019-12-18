using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Application.DataContracts;

namespace ProcessManagerSample.Queries
{
    public class GetClientQuery : Query<Client>
    {
      
    }

    public class GetClientQueryHandler : IRequestHandler<GetClientQuery, Client>
    {
        public Task<Client> Handle(GetClientQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Client("ion","vasile"));
        }
    }

    public class Client
    {
        public Client(string clientName, string clientCode)
        {
            ClientName = clientName;
            ClientCode = clientCode;
        }

        public string ClientName { get; set; }
        public string ClientCode { get; set; }
    }
}
