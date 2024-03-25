using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LuckyBlocks.Exceptions;

public static class ArgumentWasNullException
{
    public static void ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = default)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }
    
    [DoesNotReturn]
    private static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}