// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.Core.Abstractions.Paging
{
    public class PagedResult<TEntity>
    {
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public IEnumerable<TEntity> Values { get; }

        public PagedResult(int page, int pageSize, int totalCount, int totalPages, IEnumerable<TEntity> values)
        {
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = totalPages;
            Values = values;
        }

        public PagedResult<TDestination> Map<TDestination>(Func<TEntity, TDestination> mapperFunc)
        {
            return new PagedResult<TDestination>(Page, PageSize, TotalCount, TotalPages, Values.Select(mapperFunc));
        }
    }
}
