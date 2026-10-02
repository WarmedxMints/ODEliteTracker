namespace ODEliteTracker.Extensions
{
    public static class LinqExtensions
    {
        public static ulong Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> summer)
        {
            ulong total = 0;

            foreach (var item in source)
                total += summer(item);

            return total;
        }
    }
}
