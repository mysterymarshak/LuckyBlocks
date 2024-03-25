using System;
using System.IO;
using System.Reflection;

namespace LuckyBlocks.SourceGenerators;

internal static class EmbeddedResource
{
    public static string GetContent(string relativePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var baseName = assembly.GetName().Name;
        var resourceName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        using var stream = assembly
            .GetManifestResourceStream($"{baseName}.{resourceName}");

        if (stream is null)
            throw new NotSupportedException();

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}