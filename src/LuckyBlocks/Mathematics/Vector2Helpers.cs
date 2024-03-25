using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Mathematics;

internal static class Vector2Helpers
{
    public static double GetAngleBetween(Vector2 a, Vector2 b)
    {
        return Math.Acos(GetCosBetween(a, b));
    }

    public static double GetCosBetween(Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / a.Length() * b.Length();
    }
    
    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    } 
}