using LuckyBlocks.Data;
using LuckyBlocks.Entities;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal abstract class NonAreaMagicBase : MagicBase, INonAreaMagic
{
    public abstract override string Name { get; }
    
    protected Player Wizard { get; }

    protected NonAreaMagicBase(Player wizard, BuffConstructorArgs args) : base(args)
        => (Wizard) = (wizard);

    public abstract void Cast();
}