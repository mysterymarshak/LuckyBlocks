using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class Vector2Extensions
{
    public static Vector2 Rotate(this Vector2 source, double angle)
    {
        return new Vector2((float)((source.X * Math.Cos(angle)) - (source.Y * Math.Sin(angle))),
            (float)((source.X * Math.Sin(angle)) + (source.Y * Math.Cos(angle))));
    }
}