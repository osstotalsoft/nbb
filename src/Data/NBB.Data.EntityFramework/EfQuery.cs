﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace NBB.Data.EntityFramework
{
    public class EfQuery<TEntity, TContext> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
        where TContext : DbContext where TEntity : class
    {
        private readonly TContext _c;
        private readonly IQueryable<TEntity> _q;

        public EfQuery(TContext c)
        {
            _c = c;
            _c.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _q = _c.Set<TEntity>();
        }


        public Type ElementType => _q.ElementType;

        public Expression Expression => _q.Expression;

        public IQueryProvider Provider => _q.Provider;

        

        public IEnumerator<TEntity> GetEnumerator() => _q.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _q.GetEnumerator();

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => _q.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
    }
}
