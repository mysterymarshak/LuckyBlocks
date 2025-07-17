using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Magic.NonAreaMagic;

namespace LuckyBlocks.Features.Magic;

internal interface IMagicFactory
{
    IAreaMagic CreateAreaMagic<T>(Player wizard, int direction = default) where T : IAreaMagic;

    IAreaMagic CreateAreaMagic<T>(Player wizard, BuffConstructorArgs buffConstructorArgs, int direction = default)
        where T : IAreaMagic;

    T CreateMagic<T>(Player wizard) where T : class, INonAreaMagic;
}

internal class MagicFactory : IMagicFactory
{
    private readonly MagicConstructorArgs _args;

    public MagicFactory(MagicConstructorArgs args)
    {
        _args = args;
    }

    public IAreaMagic CreateAreaMagic<T>(Player wizard, int direction = default) where T : IAreaMagic =>
        ((T)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, [wizard, _args, direction], CultureInfo.CurrentCulture))
        .DecorateWithMeleeForceModifier();

    public IAreaMagic CreateAreaMagic<T>(Player wizard, BuffConstructorArgs buffConstructorArgs,
        int direction = default) where T : IAreaMagic =>
        ((T)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, [wizard, _args, buffConstructorArgs, direction],
            CultureInfo.CurrentCulture))
        .DecorateWithMeleeForceModifier();

    public T CreateMagic<T>(Player wizard) where T : class, INonAreaMagic =>
        (T)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, [wizard, _args], CultureInfo.CurrentCulture);
}