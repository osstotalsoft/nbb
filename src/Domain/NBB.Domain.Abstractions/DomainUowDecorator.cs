// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.Domain.Abstractions
{
    public class DomainUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedAggregateRoot
    {
        private readonly IUow<TEntity> _inner;

        public DomainUowDecorator(IUow<TEntity> inner)
        {
            _inner = inner;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _inner.GetChanges();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSave();

            return _inner.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSave()
        {
            var changes = GetChanges();
            foreach (var eventedAggregateRoot in changes)
            {
                eventedAggregateRoot.MarkChangesAsCommitted(); //Required to update version
            }
        }
    }
}
