using System.Runtime.CompilerServices;
using LuckyBlocks.Utils;

namespace LuckyBlocks.Extensions;

internal static class EnumExtensions
{
    public static bool HasFlag<T>(this T value, T flagToCheck) where T : unmanaged, System.Enum =>
        Unsafe.SizeOf<T>() switch
        {
            1 => (Converter<T, byte>.Convert(value) & Converter<T, byte>.Convert(flagToCheck)) != 0,
            2 => (Converter<T, ushort>.Convert(value) & Converter<T, ushort>.Convert(flagToCheck)) != 0,
            4 => (Converter<T, uint>.Convert(value) & Converter<T, uint>.Convert(flagToCheck)) != 0,
            8 => (Converter<T, ulong>.Convert(value) & Converter<T, ulong>.Convert(flagToCheck)) != 0,
            _ => throw new System.ArgumentOutOfRangeException()
        };
}