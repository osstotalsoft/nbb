using System;
using MediatR;

namespace NBB.Core.Abstractions
{
    public interface IQuery
    {
        Guid QueryId { get; }
 
        Type GetResponseType();
    }

    public interface IQuery<out TResponse> : IQuery, IRequest<TResponse>
    {
        
    }
}
