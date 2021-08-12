﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions
{
    public interface ICrudRepository<TEntity> : IReadOnlyRepository<TEntity>, IUowRepository<TEntity>
        where TEntity : class
    {
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task Update(TEntity entity, CancellationToken cancellationToken = default);
        Task Remove(object id, CancellationToken cancellationToken = default);
    }
}
