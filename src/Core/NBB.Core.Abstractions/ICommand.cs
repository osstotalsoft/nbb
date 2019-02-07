using MediatR;
using System;
using System.Collections.Generic;

namespace NBB.Core.Abstractions
{
    public interface ICommand : IRequest
    {
        Guid CommandId { get; }
    }
}
