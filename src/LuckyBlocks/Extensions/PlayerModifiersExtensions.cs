using System.Linq;
using System.Reflection;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class PlayerModifiersExtensions
{
    private static readonly FieldInfo[] PlayerModifiersFields = typeof(PlayerModifiers).GetFields();

    public static string AsString(this PlayerModifiers playerModifiers)
    {
        return string.Join(",   ", PlayerModifiersFields.Select(x => $"{x.Name} = {x.GetValue(playerModifiers)}"));
    }

    public static PlayerModifiers Revert(this PlayerModifiers playerModifiers, PlayerModifiers backedUpModifiers)
    {
        var revertedModifiers = new PlayerModifiers();

        foreach (var field in PlayerModifiersFields)
        {
            var value = field.GetValue(playerModifiers);

            if (value is -1 or -1f)
                continue;

            field.SetValue(revertedModifiers, field.GetValue(backedUpModifiers));
        }

        return revertedModifiers;
    }

    public static PlayerModifiers Concat(this PlayerModifiers first, PlayerModifiers second)
    {
        var concatenatedModifiers = new PlayerModifiers();

        foreach (var field in PlayerModifiersFields)
        {
            var value = field.GetValue(first);

            field.SetValue(concatenatedModifiers, value);

            if (value is not -1 and not -1f)
                continue;

            field.SetValue(concatenatedModifiers, field.GetValue(second));
        }

        return concatenatedModifiers;
    }

    public static PlayerModifiers Except(this PlayerModifiers first, PlayerModifiers second)
    {
        var exceptedModifiers = new PlayerModifiers();

        foreach (var field in PlayerModifiersFields)
        {
            var value = field.GetValue(first);

            field.SetValue(exceptedModifiers, value);

            if (value is -1 or -1f)
                continue;

            var comparableValue = field.GetValue(second);

            if (PlayerModifiersAreEquals(value, comparableValue))
            {
                field.SetValue(exceptedModifiers, -1);
            }
        }

        return exceptedModifiers;
    }

    public static bool IsConflictedWith(this PlayerModifiers first, PlayerModifiers second)
    {
        foreach (var field in PlayerModifiersFields)
        {
            var value = field.GetValue(first);
            var secondValue = field.GetValue(second);

            if (value is -1 or -1f || secondValue is -1 or -1f)
                continue;

            if (PlayerModifiersAreEquals(value, secondValue))
                continue;

            return true;
        }

        return false;
    }

    private static bool PlayerModifiersAreEquals(object value1, object value2)
    {
        return (value1 is int integerValue1 && value2 is int integerValue2 && integerValue1 == integerValue2)
               || (value1 is float floatValue1 && value2 is float floatValue2 && floatValue1 == floatValue2);
    }
}