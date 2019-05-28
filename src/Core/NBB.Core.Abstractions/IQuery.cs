using System;
using MediatR;

namespace NBB.Core.Abstractions
{
    public interface IQuery
    {
        Type GetResponseType();
    }

    public interface IQuery<out TResponse> : IQuery, IRequest<TResponse>
    {
        
    }
}
