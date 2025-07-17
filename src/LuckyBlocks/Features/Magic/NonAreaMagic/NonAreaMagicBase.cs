using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal abstract class NonAreaMagicBase : MagicBase, INonAreaMagic
{
    protected NonAreaMagicBase(Player wizard, MagicConstructorArgs args) : base(wizard, args)
    {
    }

    public sealed override IMagic Clone()
    {
        return base.Clone();
    }
}