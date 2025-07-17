using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;

namespace LuckyBlocks.Features.Buffs;

internal interface IBuffFactory
{
    IBuff CreateBuff<T>(Player player, TimeSpan duration = default) where T : class, IBuff;
    IBuff CreateBuff(Player player, Type type, TimeSpan duration = default);
}

internal class BuffFactory : IBuffFactory
{
    private readonly BuffConstructorArgs _buffConstructorArgs;

    public BuffFactory(BuffConstructorArgs buffConstructorArgs)
        => (_buffConstructorArgs) = (buffConstructorArgs);

    public IBuff CreateBuff<T>(Player player, TimeSpan duration = default) where T : class, IBuff
    {
        return CreateBuff(player, typeof(T), duration);
    }

    public IBuff CreateBuff(Player player, Type type, TimeSpan duration = default)
    {
        return (IBuff)Activator.CreateInstance(type,
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null,
            duration == default
                ? new object[] { player, _buffConstructorArgs }
                : new object[] { player, _buffConstructorArgs, duration }, CultureInfo.CurrentCulture);
    }
}