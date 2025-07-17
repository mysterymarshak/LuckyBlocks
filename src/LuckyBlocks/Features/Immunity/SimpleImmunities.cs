using System;

namespace LuckyBlocks.Features.Immunity;

internal record ImmunityToFreeze : IImmunity
{
    public string Name => "Immunity to freeze";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToFreeze;
}

internal record ImmunityToPoison : IImmunity
{
    public string Name => "Immunity to poison";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToPoison;
}

internal record ImmunityToShock : IDelayedRemoveImmunity
{
    public string Name => "Immunity to shock";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToShock;

    // 'init' cause security exception
    public TimeSpan RemovalDelay { get; }

    public ImmunityToShock()
    {
        RemovalDelay = TimeSpan.Zero;
    }

    public ImmunityToShock(TimeSpan removalDelay)
    {
        RemovalDelay = removalDelay;
    }
}

internal record ImmunityToWind : IImmunity
{
    public string Name => "Immunity to wind";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToWind;
}

internal record ImmunityToTimeStop : IImmunity
{
    public string Name => "Immunity to time stop";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToTimeStop;
}

internal record ImmunityToSteal : IImmunity
{
    public string Name => "Immunity to steal";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToSteal;
}

internal record ImmunityToWater : IImmunity
{
    public string Name => "Immunity to water";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToWater;
}