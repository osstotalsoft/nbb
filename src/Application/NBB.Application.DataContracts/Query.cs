using System;
using MediatR;
using NBB.Core.Abstractions;

namespace NBB.Application.DataContracts
{
    public abstract class Query<TResponse> : IQuery<TResponse>, IRequest<TResponse>, IMetadataProvider<QueryMetadata>
    {
        public QueryMetadata Metadata { get; }

        protected Query(QueryMetadata metadata = null)
        {
            Metadata = metadata ?? QueryMetadata.Default();
        }

        Type IQuery.GetResponseType() => typeof(TResponse);
    }
}
