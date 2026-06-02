namespace Swallow.Components.Utils.Internal;

internal static class EnumerableExtensions
{
    public static int IndexOfBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, TKey key) where TKey : IEquatable<TKey>
    {
        var index = 0;
        foreach (var entry in source)
        {
            var entryKey = keySelector(entry);
            if (key.Equals(entryKey))
            {
                return index;
            }

            index += 1;
        }

        return -1;
    }
}
