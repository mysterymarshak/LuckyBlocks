using System;

namespace LuckyBlocks.Utils;

internal static class SharedRandom
{
    [ThreadStatic]
    private static Random? _local;

    public static Random Instance => _local ??= new Random();
}