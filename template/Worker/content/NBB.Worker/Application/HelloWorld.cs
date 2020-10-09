using MediatR;
using NBB.Core.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Worker.Application
{
    public class HelloWorld
    {
        public class Command : ICommand, IRequest
        {

        }

        public class Handler : IRequestHandler<Command>
        {
            public Task Handle(Command request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
