using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Buffs.Finishable;
using LuckyBlocks.Features.Buffs.Wizards;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.WeaponPowerups.Bullets;
using LuckyBlocks.Loot.Attributes;
using NetEscapades.EnumGenerators;

namespace LuckyBlocks.Loot;

[EnumExtensions]
internal enum Item
{
    [Unused]
    None = 0,

    [DeadPlayerExists]
    RespawnRandomPlayer,

    LegendaryWeapon,

    Explosion,

    [OnlyPlayer]
    [PlayerIsNotFullHp]
    FullHp,

    [OnlyPlayer]
    [PlayerHasNoImmunities(ImmunityFlag.ImmunityToFreeze)]
    Freeze,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(StrongMan), nameof(Features.Buffs.Durable.StrongMan.ModifiedModifiers))]
    StrongMan,

    [BarrelExists]
    ExplodeRandomBarrel,

    Medkit,

    [LuckyBlockDropChanceCanBeIncreased]
    IncreaseSpawnChance,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(HighJumps), nameof(Features.Buffs.Durable.HighJumps.ModifiedModifiers))]
    HighJumps,

    [AlivePlayersMoreThanOne]
    ShufflePositions,

    [AlivePlayersMoreThanOne]
    [AnyPlayerHaveAnyWeapon]
    ShuffleWeapons,

    StickyGrenades,

    BananaGrenades,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Vampirism), nameof(Features.Buffs.Durable.Vampirism.ModifiedModifiers))]
    Vampirism,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(WindWizard))]
    WindWizard,

    IgniteRandomPlayer,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Shield), nameof(Features.Buffs.Durable.Shield.ModifiedModifiers))]
    Shield,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Hulk), nameof(Features.Buffs.Durable.Hulk.ModifiedModifiers))]
    Hulk,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Dwarf), nameof(Features.Buffs.Durable.Dwarf.ModifiedModifiers))]
    Dwarf,

    [Weight(0.5f)]
    BloodyBath,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(FireWizard))]
    FireWizard,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(ElectricWizard))]
    ElectricWizard,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(ExplosiveBullets))]
    ExplosiveBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(TripleRicochetBullets))]
    TripleRicochetBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(FreezeBullets))]
    FreezeBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(PushBullets))]
    PushBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(AimBullets))]
    AimBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(InfiniteRicochetBullets))]
    InfiniteRicochetBullets,

    [OnlyPlayer]
    [NoOneHaveBuff(typeof(DecoyWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(DecoyWizard))]
    DecoyWizard,

    [OnlyPlayer]
    [PlayerDoesNotHaveBuff(typeof(TotemOfUndying))]
    TotemOfUndying,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(TimeStopWizard))]
    TimeStopWizard,

    WeaponWithRandomPowerups,

    FlamyKatana,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(SourcePowerup = typeof(PoisonBullets))]
    PoisonBullets,

    [OnlyPlayer]
    [AlivePlayersMoreThanOne]
    [Weight(0.5f / 2)]
    [AnyPlayerHaveAnyWeapon(true)]
    RemoveWeaponsExceptActivator,

    [Weight(0.5f / 2)]
    [AnyPlayerHaveAnyWeapon]
    RemoveWeapons,

    [OnlyPlayer]
    [PlayerDoesNotHaveBuff(typeof(RestoreWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(RestoreWizard))]
    RestoreWizard,

    [OnlyPlayer]
    [PlayerDoesNotHaveBuff(typeof(StealWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(StealWizard))]
    StealWizard,

    FunWeapon,

    [OnlyPlayer]
    [NoOneHaveBuff(typeof(TimeRevertWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(TimeRevertWizard))]
    TimeRevertWizard,

    [OnlyPlayer]
    [PlayerHasNoImmunities(ImmunityFlag.ImmunityToWater)]
    WetHands,

    [OnlyPlayer]
    [NoOneHaveBuff(typeof(TheFool))]
    [MagicIsAllowed]
    TheFool
}