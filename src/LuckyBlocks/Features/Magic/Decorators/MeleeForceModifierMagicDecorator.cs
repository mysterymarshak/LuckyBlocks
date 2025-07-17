using System;
using LuckyBlocks.Data;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic.AreaMagic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.Decorators;

internal class MeleeForceModifierMagicDecorator : IAreaMagic
{
    public event Action<IAreaMagic>? Iterate
    {
        add => _magic.Iterate += value;
        remove => _magic.Iterate -= value;
    }

    public string Name => _magic.Name;
    public bool IsCloned => _magic.IsCloned;
    public bool IsFinished => _magic.IsFinished;
    public bool ShouldCastOnRestore => _magic.ShouldCastOnRestore;
    public Player Wizard => _magic.Wizard;
    public AreaMagicType Type => _magic.Type;
    public IFinishCondition<IMagic> WhenFinish => _magic.WhenFinish;
    public Vector2 AreaSize => new(_magic.AreaSize.X * (float)GetStrengthModifier(), _magic.AreaSize.Y);
    public int IterationsCount => (int)Math.Round(_magic.IterationsCount * (float)GetStrengthModifier());
    public TimeSpan PropagationTime => _magic.PropagationTime.Multiply(1 / GetStrengthModifier());
    public int Direction => _magic.Direction;

    private readonly IAreaMagic _magic;

    public MeleeForceModifierMagicDecorator(IAreaMagic sourceMagic)
        => _magic = sourceMagic;

    public IMagic Clone() => ((IAreaMagic)_magic.Clone()).DecorateWithMeleeForceModifier();
    public void OnRestored() => _magic.OnRestored();
    public void Cast() => _magic.Cast();
    public void ExternalFinish() => _magic.ExternalFinish();
    public void Reflect() => _magic.Reflect();
    public Area GetCurrentIteration() => _magic.GetCurrentIteration();
    public Area GetFullArea() => _magic.GetFullArea();
    public void Cast(Area fullArea, Area iterationArea) => _magic.Cast(fullArea, iterationArea);
    public void PlayEffects(Area area) => _magic.PlayEffects(area);

    public override bool Equals(object? obj)
    {
        return _magic.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _magic.GetHashCode();
    }

    private double GetStrengthModifier()
    {
        var wizardInstance = Wizard.Instance;
        ArgumentWasNullException.ThrowIfNull(wizardInstance);

        var wizardModifiers = wizardInstance.GetModifiers();
        return MathHelper.Clamp(wizardModifiers.MeleeForceModifier, 0.5f, 3f);
    }
}