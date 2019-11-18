using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.Abstractions.Linq
{
    public static class QueryableAsyncExtensions
    {
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => source.AsAsyncEnumerable().ToList(cancellationToken);

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => source.Take(1).AsAsyncEnumerable().FirstOrDefault(cancellationToken);

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
            => source.Where(predicate).Take(1).AsAsyncEnumerable().FirstOrDefault(cancellationToken);

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => source.Take(1).AsAsyncEnumerable().First(cancellationToken);

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
            => source.Where(predicate).Take(1).AsAsyncEnumerable().First(cancellationToken);

        public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
            => source.Where(predicate).Skip(Math.Max(0, source.Count() - 1)).AsAsyncEnumerable().Last(cancellationToken);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => source.Take(2).AsAsyncEnumerable().SingleOrDefault(cancellationToken);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
            => source.Where(predicate).Take(2).AsAsyncEnumerable().SingleOrDefault(cancellationToken);

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => source.Take(2).AsAsyncEnumerable().Single(cancellationToken);

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
            => source.Where(predicate).Take(2).AsAsyncEnumerable().Single(cancellationToken);

        public static Task ForEachAsync<TSource>(this IQueryable<TSource> source, Action<TSource> action,
            CancellationToken cancellationToken = default)
            => source.AsAsyncEnumerable().ForEachAsync(action, cancellationToken);

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            => source.AsAsyncEnumerable().ToDictionary(keySelector, cancellationToken);


        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            => source.AsAsyncEnumerable().ToDictionary(keySelector, elementSelector, cancellationToken);

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            => source.AsAsyncEnumerable().ToDictionary(keySelector, comparer, cancellationToken);

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            => source.AsAsyncEnumerable().ToDictionary(keySelector, elementSelector, comparer, cancellationToken);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => source.Select(x => source.Count()).Take(1).AsAsyncEnumerable().Single(cancellationToken);


        //public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => source.Skip(Math.Max(0, source.Count() - 1)).AsAsyncEnumerable().LastOrDefault(cancellationToken);

        //public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
        //    Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        //    => source.Where(predicate).Skip(Math.Max(0, source.Count() - 1)).AsAsyncEnumerable().LastOrDefault(cancellationToken);

        //public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => source.Skip(Math.Max(0, source.Count() - 1)).AsAsyncEnumerable().Last(cancellationToken);

        //public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => Task.FromResult(source.Min());

        //public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source,
        //    Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Min(selector, cancellationToken);

        //public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Min(cancellationToken);

        //public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source,
        //    Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Min(selector, cancellationToken);

        //public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<decimal> AverageAsync(this IQueryable<decimal> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double> AverageAsync(this IQueryable<int> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double?> AverageAsync(this IQueryable<int?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int?> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double> AverageAsync(this IQueryable<long> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double?> AverageAsync(this IQueryable<long?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double> AverageAsync(this IQueryable<double> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double?> AverageAsync(this IQueryable<double?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, double> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<float> AverageAsync(this IQueryable<float> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<float?> AverageAsync(this IQueryable<float?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Any(cancellationToken);

        //public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Contains(item, cancellationToken);

        //public static Task<bool> AllAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().All(predicate, cancellationToken);

        //public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Count(cancellationToken);

        //public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Count(predicate, cancellationToken);

        //public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().LongCount(cancellationToken);

        //public static Task<long> CountLongCountAsyncAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().LongCount(predicate, cancellationToken);

        //public static Task<decimal?> SumAsync(this IQueryable<decimal?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<decimal> SumAsync(this IQueryable<decimal> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, decimal> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source,
        //    Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double?> SumAsync(this IQueryable<int?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, int?> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double?> SumAsync(this IQueryable<long?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, long?> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double> SumAsync(this IQueryable<double> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double?> SumAsync(this IQueryable<double?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, double> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, double?> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<float?> SumAsync(this IQueryable<float?> source,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(cancellationToken);

        //public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);

        //public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source, Func<TSource, float?> selector,
        //    CancellationToken cancellationToken = default)
        //    => source.AsAsyncEnumerable().Average(selector, cancellationToken);


        private static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
        {
            if (source is IAsyncEnumerable<TSource> asyncEnumerable)
                return asyncEnumerable;

            throw new NotSupportedException();
        }
    }
}
