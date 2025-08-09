using System;
using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace LuckyBlocks.Features.Immunity;

[Flags]
[EnumExtensions]
internal enum ImmunityFlag
{
    None = 0,

    [Display(Name = "Poison immunity")]
    ImmunityToPoison = 1,

    [Display(Name = "Fire immunity")]
    ImmunityToFire = 2,

    [Display(Name = "Freeze immunity")]
    ImmunityToFreeze = 4,

    [Display(Name = "Wind immunity")]
    ImmunityToWind = 8,

    [Display(Name = "Shock immunity")]
    ImmunityToShock = 16,

    [Display(Name = "Time stop immunity")]
    ImmunityToTimeStop = 32,

    [Display(Name = "Magic immunity")]
    ImmunityToMagic = ImmunityToFire | ImmunityToFreeze | ImmunityToWind | ImmunityToShock,

    [Display(Name = "Fall immunity")]
    ImmunityToFall = 64,

    [Display(Name = "Damage immunity")]
    FullDamageImmunity = ImmunityToPoison | ImmunityToMagic | ImmunityToFall,

    [Display(Name = "Steal immunity")]
    ImmunityToSteal = 128,

    [Display(Name = "Water immunity")]
    ImmunityToWater = 256
}