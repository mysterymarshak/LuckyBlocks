using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace LuckyBlocks.SourceGenerators.ExtendedEvents;

internal static class ParameterSymbolExtensions
{
    public static bool IsDelegate(this IParameterSymbol parameterSymbol)
    {
        return parameterSymbol is { Type.Name: nameof(Action) or "Func" };
    }
}

internal static class TypeExtensions
{
    public static string GetUnderlyingName(this Type type)
    {
        return type.IsArray ? type.GetElementType()!.Name : type.Name;
    }
}

internal static class TypeSymbolExtensions
{
    public static string GetUnderlyingName(this ITypeSymbol typeSymbol)
    {
        return typeSymbol is IArrayTypeSymbol arrayTypeSymbol
            ? arrayTypeSymbol.ElementType.Name
            : typeSymbol.Name;
    }

    public static bool IsDelegate(this ITypeSymbol typeSymbol)
    {
        return typeSymbol is { Name: nameof(Action) or "Func" };
    }
}

internal static class MethodSymbolExtensions
{
    public static bool HasAttribute(this IMethodSymbol methodSymbol, string attributeName)
    {
        return methodSymbol.GetAttributes().Any(y => y.AttributeClass!.Name == attributeName);
    }
}