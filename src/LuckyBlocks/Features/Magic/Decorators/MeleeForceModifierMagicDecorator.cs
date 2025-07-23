using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Magic.AreaMagic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.Decorators;

internal class MeleeForceModifierMagicDecorator : AreaMagicBase
{
    public override string Name => _magic.Name;
    public override AreaMagicType Type => _magic.Type;
    public override Vector2 AreaSize => new(_magic.AreaSize.X * (float)GetStrengthModifier(), _magic.AreaSize.Y);
    public override int IterationsCount => (int)Math.Round(_magic.IterationsCount * (float)GetStrengthModifier());
    public override TimeSpan PropagationTime => _magic.PropagationTime.Multiply(1 / GetStrengthModifier());

    private readonly AreaMagicBase _magic;
    private readonly MagicConstructorArgs _args;

    public MeleeForceModifierMagicDecorator(AreaMagicBase sourceMagic, MagicConstructorArgs args,
        int direction = default) : base(sourceMagic.Wizard, args, direction)
    {
        _magic = sourceMagic;
        _args = args;
    }

    public override void PlayEffects(Area area)
    {
        _magic.PlayEffects(area);
    }

    public override void Cast(Area fullArea, Area iterationArea)
    {
        _magic.Cast(fullArea, iterationArea);
        base.Cast(fullArea, iterationArea);
    }

    public override MagicBase Copy()
    {
        return (MagicBase)((AreaMagicBase)_magic.Copy()).DecorateWithMeleeForceModifier(_args, _magic.Direction);
    }

    private double GetStrengthModifier()
    {
        var wizardInstance = Wizard.Instance;
        ArgumentWasNullException.ThrowIfNull(wizardInstance);

        var wizardModifiers = wizardInstance.GetModifiers();
        return MathHelper.Clamp(wizardModifiers.MeleeForceModifier, 0.5f, 3f);
    }
}