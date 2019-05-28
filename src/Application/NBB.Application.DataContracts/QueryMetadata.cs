using System;

namespace NBB.Application.DataContracts
{
    public class QueryMetadata
    {
        public Guid QueryId { get; }
        public DateTime CreationDate { get; }

        public QueryMetadata(Guid queryId, DateTime creationDate)
        {
            QueryId = queryId;
            CreationDate = creationDate;
        }

        public static QueryMetadata Default() => new QueryMetadata(Guid.NewGuid(), DateTime.UtcNow);
    }
}
