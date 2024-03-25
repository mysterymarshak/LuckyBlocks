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
    public TimeSpan RemovalDelay => TimeSpan.FromSeconds(2);
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