using System;
using System.Linq.Expressions;

namespace LuckyBlocks.Utils;

internal static class Converter<T1, T2>
{
    private static readonly Func<T1, T2> CachedFunc = GenerateConverterFunc();

    public static T2 Convert(T1 value)
    {
        return CachedFunc(value);
    }

    private static Func<T1, T2> GenerateConverterFunc()
    {
        var inputParameter = Expression.Parameter(typeof(T1));
        var body = Expression.Convert(inputParameter, typeof(T2));
        var lambda = Expression.Lambda<Func<T1, T2>>(body, inputParameter);
        return lambda.Compile();
    }
}