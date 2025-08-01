using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class IScriptStorageExtensions
{
    public static bool TryGetValueOrDefault(this IScriptStorage storage, string key, bool defaultValue)
    {
        return storage.TryGetItemBool(key, out var value) ? value : defaultValue;
    }

    public static float TryGetValueOrDefault(this IScriptStorage storage, string key, float defaultValue)
    {
        return storage.TryGetItemFloat(key, out var value) ? value : defaultValue;
    }

    public static IEnumerable<string> TryGetValueOrDefault(this IScriptStorage storage, string key,
        IEnumerable<string> defaultValue)
    {
        return storage.TryGetItemStringArr(key, out var value) ? value : defaultValue;
    }
}