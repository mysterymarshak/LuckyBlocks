using System;
using System.Collections.Generic;

namespace LuckyBlocks.Extensions;

internal static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        value = valueFactory(key);
        dictionary[key] = value;
        
        return value;
    }
    
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        
        value = valueFactory();
        dictionary[key] = value;

        return value;
    }
}