namespace MT5Bridge.Core.Collections;

public static class DictUtil
{
    /// <summary>
    /// Gets value from dictionary and removes the key-value pair.
    /// Returns defaultValue if key doesn't exist.
    /// </summary>
    public static TValue GetThenRemove<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        where TKey : notnull
    {
        if (dict.Remove(key, out TValue? value))
        {
            return value!;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets value from dictionary with a default value if key doesn't exist.
    /// </summary>
    public static TValue GetWithDefault<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out TValue? value))
        {
            return value!;
        }

        return defaultValue;
    }

    /// <summary>
    /// Converts ConcurrentDictionary to regular Dictionary.
    /// </summary>
    public static Dictionary<string, string> ConvertToDictionary(System.Collections.Concurrent.ConcurrentDictionary<string, string> concurrentDict)
    {
        return new Dictionary<string, string>(concurrentDict);
    }
}
