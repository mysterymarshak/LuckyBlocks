using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckyBlocks.Utils;

internal static class EnumUtils
{
    public static bool AttributeExist<T1, T2>(T2 item) where T1 : Attribute where T2 : Enum => GetAttributesOfType<T1, T2>(item).Any();

    public static IEnumerable<T1> GetAttributesOfType<T1, T2>(T2 item) where T1 : Attribute where T2 : Enum
    {
        var enumType = item.GetType();
        var memberInfo = enumType.GetMember(Enum.GetName(enumType, item)!).First();
        var valueAttributes = memberInfo.GetCustomAttributes(typeof(T1), false);
        return valueAttributes.Cast<T1>();
    }

    public static IEnumerable<T> GetSingleFlags<T>(T enumValue) where T : Enum
    {
        var intValue = Converter<T, int>.Convert(enumValue);
        var maxFlagsInValue = (int)Math.Floor(Math.Log(intValue, 2) + 1);
        var flags = new List<T>(maxFlagsInValue);

        for (var i = 0; i < maxFlagsInValue; i++)
        {
            var flag = 1 << i;

            if ((intValue & flag) == 0)
                continue;

            flags.Add(Converter<int, T>.Convert(flag));
        }

        return flags;
    }
}