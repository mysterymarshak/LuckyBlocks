using System;

namespace LuckyBlocks.Features.Immunity;

[Flags]
internal enum ImmunityFlag
{
    None = 0,

    ImmunityToPoison = 1,

    ImmunityToFire = 2,

    ImmunityToFreeze = 4,
    
    ImmunityToWind = 8,
    
    ImmunityToShock = 16,
    
    ImmunityToTimeStop = 32,

    ImmunityToMagic = ImmunityToFire | ImmunityToFreeze | ImmunityToWind | ImmunityToShock,

    ImmunityToFall = 64,
    
    ImmunityToDeath = 128,
        
    FullDamageImmunity = ImmunityToPoison | ImmunityToMagic | ImmunityToFall
}