using System;
using NBB.Core.Abstractions;

namespace NBB.Application.DataContracts
{
    public abstract class Query<TResponse> : IQuery<TResponse>, IMetadataProvider<QueryMetadata>
    {
        public QueryMetadata Metadata { get; }

        protected Query(QueryMetadata metadata)
        {
            Metadata = metadata ?? QueryMetadata.Default();
        }

        Type IQuery.GetResponseType() => typeof(TResponse);
    }
}
