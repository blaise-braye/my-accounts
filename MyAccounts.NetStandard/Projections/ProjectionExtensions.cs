using System.Collections.Generic;

namespace MyAccounts.NetStandard.Projections
{
    public static class ProjectionExtensions
    {
        public static CollectionProjectionBuilder<TSource> Project<TSource>(this IEnumerable<TSource> source)
        {
            return new CollectionProjectionBuilder<TSource>(source);
        }

        public static ItemProjectionBuilder<TSource> Map<TSource>(this TSource source)
        {
            return new ItemProjectionBuilder<TSource>(source);
        }
    }
}