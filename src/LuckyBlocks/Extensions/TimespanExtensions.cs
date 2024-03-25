using System;

namespace LuckyBlocks.Extensions;

internal static class TimespanExtensions
{
    public static TimeSpan Multiply(this TimeSpan timeSpan, double modifier)
    {
        return TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds * modifier);
    }
    
    public static TimeSpan Divide(this TimeSpan timeSpan, double modifier)
    {
        return TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds / modifier);
    }
    
    public static double Divide(this TimeSpan timeSpan1, TimeSpan timeSpan2)
    {
        return timeSpan1.TotalMilliseconds / timeSpan2.TotalMilliseconds;
    }
}