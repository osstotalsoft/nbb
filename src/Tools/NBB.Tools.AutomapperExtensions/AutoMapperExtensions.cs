using AutoMapper;
using AutoMapper.Internal;
using System;
using System.Linq.Expressions;

namespace NBB.Tools.AutoMapperExtensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> ForCtorParamMatching<TSource, TDestination, TMember>(
            this IMappingExpression<TSource, TDestination> mappingExpression,
            Expression<Func<TDestination, TMember>> destinationMember,
            Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
        {
            var memberInfo = ReflectionHelper.FindProperty(destinationMember);
            var camelCaseParam = char.ToLowerInvariant(memberInfo.Name[0]) + memberInfo.Name.Substring(1);

            return mappingExpression.ForCtorParam(camelCaseParam, paramOptions);
        }
    }
}
