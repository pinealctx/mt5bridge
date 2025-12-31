using System;

namespace MT5Bridge.Core.Collections;

public static class ArrayUtils
{
    /// <summary>
    /// Efficiently converts a source collection (accessed by index) to an array,
    /// with optional filtering and conversion.
    /// Minimizes allocations by pre-allocating the array and resizing only if necessary.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="count">The number of items to process.</param>
    /// <param name="getItem">Function to retrieve an item by index (0 to count-1).</param>
    /// <param name="converter">Function to convert a source item to a result item.</param>
    /// <param name="filter">Optional function to filter items. If null, all non-null items are included.</param>
    /// <returns>An array of converted items.</returns>
    public static TResult[] Convert<TSource, TResult>(
        uint count,
        Func<uint, TSource?> getItem,
        Func<TSource, TResult> converter,
        Func<TSource, bool>? filter = null)
    {
        if (count == 0)
        {
            return Array.Empty<TResult>();
        }

        var results = new TResult[count];
        int resultCount = 0;

        for (uint i = 0; i < count; i++)
        {
            var item = getItem(i);
            if (item is not null)
            {
                if (filter == null || filter(item))
                {
                    results[resultCount++] = converter(item);
                }
            }
        }

        if (resultCount < count)
        {
            Array.Resize(ref results, resultCount);
        }

        return results;
    }
}
