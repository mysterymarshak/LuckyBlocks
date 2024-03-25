using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Mathematics;

internal readonly record struct Triangle
{
    private readonly Vector2 _a;
    private readonly Vector2 _b;
    private readonly Vector2 _c;

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
        => (_a, _b, _c) = (a, b, c);

    // https://www.geeksforgeeks.org/check-whether-a-given-point-lies-inside-a-triangle-or-not/
    public bool IsInTriangle(Vector2 p)
    {
        var abc = GetArea(_a, _b, _c);
        var pbc = GetArea(p, _b, _c);
        var pac = GetArea(p, _a, _c);
        var pab = GetArea(p, _a, _b);

        return Math.Abs(abc - (pbc + pac + pab)) < 0.1f;
    }

    private static float GetArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return Math.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2;
    }
}