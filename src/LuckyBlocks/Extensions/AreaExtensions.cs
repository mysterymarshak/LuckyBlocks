using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class AreaExtensions
{
    public static float GetDiagonalLength(this Area area)
    {
        return Vector2.Distance(area.Min, area.Max);
    }
}