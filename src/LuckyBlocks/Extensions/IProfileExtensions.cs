using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class IProfileExtensions
{
    private static readonly Dictionary<string, FieldInfo> FieldsInfo = new();

    public static FieldInfo GetField(string name)
    {
        return FieldsInfo.GetOrAdd(name, name => typeof(IProfile).GetField(name));
    }

    public static IEnumerable<string> GetProperties()
    {
        return typeof(IProfile).GetFields()
            .Where(x => x.FieldType == typeof(IProfileClothingItem))
            .Select(x => x.Name)
            .Where(x => x is not (nameof(IProfile.Gender) or nameof(IProfile.Name)));
    }

    public static IProfileClothingItem? Clone(this IProfileClothingItem? originalClothingItem, string? colorName = null)
    {
        if (originalClothingItem is null)
        {
            return null;
        }

        return new IProfileClothingItem(originalClothingItem.Name, colorName ?? originalClothingItem.Color1,
            colorName ?? originalClothingItem.Color2, colorName ?? originalClothingItem.Color3);
    }
}