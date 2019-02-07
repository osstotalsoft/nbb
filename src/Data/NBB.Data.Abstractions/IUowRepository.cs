using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Data.Abstractions
{
    public interface IUowRepository<out TEntity>
        where TEntity : class
    {
        IUow<TEntity> Uow { get; }
    }
}
