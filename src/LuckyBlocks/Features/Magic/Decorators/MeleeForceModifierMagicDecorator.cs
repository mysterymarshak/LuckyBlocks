using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Magic.AreaMagic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.Decorators;

internal class MeleeForceModifierMagicDecorator : IAreaMagic
{
    public string Name => _magic.Name;
    public AreaMagicType Type => _magic.Type;
    public IFinishCondition<IMagic> WhenFinish => _magic.WhenFinish;
    public Vector2 AreaSize => new(_magic.AreaSize.X * (float)GetStrengthModifier(), _magic.AreaSize.Y);
    public int IterationsCount => (int)Math.Round(_magic.IterationsCount * (float)GetStrengthModifier());
    public TimeSpan PropagationTime => _magic.PropagationTime.Multiply(1 / GetStrengthModifier());
    public int Direction => _magic.Direction;

    private readonly IAreaMagic _magic;
    private readonly Player _wizard;

    public MeleeForceModifierMagicDecorator(IAreaMagic sourceMagic, Player wizard)
        => (_magic, _wizard) = (sourceMagic, wizard);

    public void ExternalFinish() => _magic.ExternalFinish();
    public void Reflect() => _magic.Reflect();
    public void Cast(Area area) => _magic.Cast(area);
    public void PlayEffects(Area area) => _magic.PlayEffects(area);

    private double GetStrengthModifier()
    {
        var wizardInstance = _wizard.Instance;
        ArgumentWasNullException.ThrowIfNull(wizardInstance);

        var wizardModifiers = wizardInstance.GetModifiers();
        return MathHelper.Clamp(wizardModifiers.MeleeForceModifier, 0.5f, 3f);
    }
}