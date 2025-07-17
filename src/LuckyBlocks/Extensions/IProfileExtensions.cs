using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class IProfileExtensions
{
    public static IProfile ToSingleColor(this IProfile originalProfile, string colorName)
    {
        return new IProfile
        {
            Accesory = new IProfileClothingItem(originalProfile.Accessory?.Name, colorName, colorName),
            ChestOver = new IProfileClothingItem(originalProfile.ChestOver?.Name, colorName, colorName),
            ChestUnder = new IProfileClothingItem(originalProfile.ChestUnder?.Name, colorName, colorName),
            Feet = new IProfileClothingItem(originalProfile.Feet?.Name, colorName, colorName),
            Hands = new IProfileClothingItem(originalProfile.Hands?.Name, colorName, colorName),
            Legs = new IProfileClothingItem(originalProfile.Legs?.Name, colorName, colorName),
            Skin = new IProfileClothingItem(originalProfile.Skin?.Name, colorName, colorName),
            Waist = new IProfileClothingItem(originalProfile.Waist?.Name, colorName, colorName),
            Head = new IProfileClothingItem(originalProfile.Head?.Name, colorName, colorName),
            Gender = originalProfile.Gender,
            Name = originalProfile.Name
        };
    }

    public static IProfile Clone(this IProfile originalProfile)
    {
        return new IProfile
        {
            Accesory = originalProfile.Accesory?.Clone(),
            ChestOver = originalProfile.ChestOver?.Clone(),
            ChestUnder = originalProfile.ChestUnder?.Clone(),
            Feet = originalProfile.Feet?.Clone(),
            Hands = originalProfile.Hands?.Clone(),
            Legs = originalProfile.Legs?.Clone(),
            Skin = originalProfile.Skin?.Clone(),
            Waist = originalProfile.Waist?.Clone(),
            Head = originalProfile.Head?.Clone(),
            Gender = originalProfile.Gender,
            Name = originalProfile.Name
        };
    }

    private static IProfileClothingItem? Clone(this IProfileClothingItem? originalClothingItem)
    {
        if (originalClothingItem is null)
        {
            return null;
        }
        
        return new IProfileClothingItem(originalClothingItem.Name, originalClothingItem.Color1, originalClothingItem.Color2, originalClothingItem.Color3);
    }
}