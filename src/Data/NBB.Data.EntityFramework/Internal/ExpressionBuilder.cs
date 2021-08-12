// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NBB.Data.EntityFramework.Internal
{
    public class KeyClass
    {
        public object id;
    }

    public class ExpressionBuilder : IExpressionBuilder
    {
        public Expression<Func<TEntity, bool>> BuildPrimaryKeyExpressionFromModel<TEntity>(IModel model, object keyValues)
            where TEntity : class
        {
            IList<object> values = (keyValues is object[] list) ? list.ToList() : new List<object> {keyValues};

            var keyProperties = model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties;
            var lambda = BuildPrimaryKeyExpression<TEntity>(keyProperties, values);

            return lambda;
        }


        public Expression<Func<TEntity, bool>> BuildPrimaryKeyExpression<TEntity>(IReadOnlyList<IProperty> keyProperties, IList<object> keyValues)
            where TEntity : class
        {            
            var entityParam = Expression.Parameter(typeof(TEntity), "entity");

            Expression filterExpression = null;

            if (keyValues.Count > 1)
            {
                
                filterExpression =
                    Expression.AndAlso(GetExpression(entityParam, keyProperties.First(), keyValues.First()),
                        GetExpression(entityParam, keyProperties[1], keyValues[1]));
                for (int i = 2; i < keyValues.Count; i++)
                {
                    filterExpression = Expression.AndAlso(filterExpression, GetExpression(entityParam, keyProperties[i], keyValues[i]));
                }
            }
            else
            {
                filterExpression = GetExpression(entityParam, keyProperties.First(), keyValues.First());
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(
                filterExpression, entityParam);

            return lambda;
        }

        private Expression GetExpression(ParameterExpression entityParam, IProperty keyProperty, object keyValue)
        {
            return Expression.Equal(
                Expression.PropertyOrField(entityParam, keyProperty.Name),
                Expression.Convert(
                    Expression.Field(
                        Expression.Constant(new KeyClass { id = keyValue }),
                        nameof(KeyClass.id)
                    ),
                    keyProperty.ClrType
                )
            );
        }
    }
}
