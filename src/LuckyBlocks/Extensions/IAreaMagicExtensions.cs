using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Magic.Decorators;

namespace LuckyBlocks.Extensions;

internal static class IAreaMagicExtensions
{
    public static IAreaMagic DecorateWithMeleeForceModifier(this AreaMagicBase magic, MagicConstructorArgs args,
        int direction = default) => new MeleeForceModifierMagicDecorator(magic, args, direction);
}