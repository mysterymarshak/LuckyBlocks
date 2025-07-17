using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Magic.Decorators;

namespace LuckyBlocks.Extensions;

internal static class IAreaMagicExtensions
{
    public static IAreaMagic DecorateWithMeleeForceModifier(this IAreaMagic magic) =>
        new MeleeForceModifierMagicDecorator(magic);
}