// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NBB.Data.EntityFramework.Internal
{
    public interface IExpressionBuilder
    {
        Expression<Func<TEntity, bool>> BuildPrimaryKeyExpressionFromModel<TEntity>(IModel model, object keyValues)
            where TEntity : class;
    }
}
