using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions.Linq
{
    public static class QueryableAsyncExtensions
    {
        public static ValueTask<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ToListAsync(cancellationToken);

        public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().FirstOrDefaultAsync(cancellationToken);

        public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().FirstOrDefaultAsync(predicate, cancellationToken);

        public static ValueTask<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().FirstAsync(cancellationToken);

        public static ValueTask<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().FirstAsync(predicate, cancellationToken);

        public static ValueTask<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().LastOrDefaultAsync(cancellationToken);

        public static ValueTask<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().LastOrDefaultAsync(predicate, cancellationToken);

        public static ValueTask<TSource> LastAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().LastAsync(cancellationToken);

        public static ValueTask<TSource> LastAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().LastAsync(predicate, cancellationToken);

        public static ValueTask<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().SingleOrDefaultAsync(cancellationToken);

        public static ValueTask<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().SingleOrDefaultAsync(predicate, cancellationToken);

        public static ValueTask<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().SingleAsync(cancellationToken);

        public static ValueTask<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().SingleAsync(predicate, cancellationToken);

        public static ValueTask<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().MinAsync(cancellationToken);

        public static ValueTask<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().MinAsync(selector, cancellationToken);

        public static ValueTask<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().MinAsync(cancellationToken);

        public static ValueTask<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().MinAsync(selector, cancellationToken);

        public static ValueTask<decimal?> AverageAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<decimal> AverageAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<decimal> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double?> AverageAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double?> AverageAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double?> AverageAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<float?> AverageAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<float> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<float?> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AnyAsync(cancellationToken);

        public static ValueTask<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ContainsAsync(item, cancellationToken);

        public static ValueTask<bool> AllAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AllAsync(predicate, cancellationToken);

        public static ValueTask<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().CountAsync(cancellationToken);

        public static ValueTask<int> CountAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().CountAsync(predicate, cancellationToken);

        public static ValueTask<long> LongCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().LongCountAsync(cancellationToken);

        public static ValueTask<long> CountLongCountAsyncAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().LongCountAsync(predicate, cancellationToken);

        public static Task ForEachAsync<TSource>(this IQueryable<TSource> source, Action<TSource> action, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ForEachAsync(action, cancellationToken);

        public static ValueTask<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<decimal?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<double> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<double?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(cancellationToken);

        public static ValueTask<float> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<float?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().AverageAsync(selector, cancellationToken);

        public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ToDictionaryAsync(keySelector, cancellationToken);


        public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, cancellationToken);

        public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ToDictionaryAsync(keySelector, comparer, cancellationToken);

        public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default (CancellationToken))
            => source.ToAsyncEnumerable().ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
    }
}
