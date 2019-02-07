using System;
using NBB.Core.Abstractions;

namespace NBB.Application.DataContracts
{
    public abstract class Query<TResponse> : IQuery<TResponse>, IMetadataProvider<ApplicationMetadata>
    {
        public Guid QueryId { get; }  
        public ApplicationMetadata Metadata { get; }

        protected Query(Guid queryId, ApplicationMetadata metadata)
        {
            QueryId = queryId;
            Metadata = metadata ?? new ApplicationMetadata();
        }

        Type IQuery.GetResponseType() => typeof(TResponse);
    }
}
