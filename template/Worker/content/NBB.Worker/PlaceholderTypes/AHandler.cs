#if MediatR
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace NBB.Worker
{
    public class __AHandler__ : IRequestHandler<__ACommand__>
    {
        public Task Handle(__ACommand__ request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
