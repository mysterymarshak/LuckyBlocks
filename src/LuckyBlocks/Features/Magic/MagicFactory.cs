using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Magic.Decorators;
using LuckyBlocks.Features.Magic.NonAreaMagic;

namespace LuckyBlocks.Features.Magic;

internal interface IMagicFactory
{
    IAreaMagic CreateAreaMagic<T>(Player wizard, BuffConstructorArgs args, int direction = default) where T : IAreaMagic;
    T CreateMagic<T>(Player wizard, BuffConstructorArgs args) where T : class, INonAreaMagic;
}

internal class MagicFactory : IMagicFactory
{
    public IAreaMagic CreateAreaMagic<T>(Player wizard, BuffConstructorArgs args, int direction = default) where T : IAreaMagic =>
        ((T)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, new object[] { wizard, args, direction }, CultureInfo.CurrentCulture))
        .DecorateWithMeleeForceModifier(wizard);

    public T CreateMagic<T>(Player wizard, BuffConstructorArgs args) where T : class, INonAreaMagic =>
        (T)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, new object[] { wizard, args }, CultureInfo.CurrentCulture);
}

file static class IAreaMagicExtensions
{
    public static IAreaMagic DecorateWithMeleeForceModifier(this IAreaMagic magic, Player wizard) =>
        new MeleeForceModifierMagicDecorator(magic, wizard);
}