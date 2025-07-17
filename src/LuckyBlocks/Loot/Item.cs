using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Buffs.Wizards;
using LuckyBlocks.Features.WeaponPowerups.Bullets;
using LuckyBlocks.Loot.Attributes;

namespace LuckyBlocks.Loot;

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
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(ExplosiveBullets))]
    ExplosiveBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(TripleRicochetBullets))]
    TripleRicochetBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(FreezeBullets))]
    FreezeBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(PushBullets))]
    PushBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(AimBullets))]
    AimBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(InfiniteRicochetBullets))]
    InfiniteRicochetBullets,

    [OnlyPlayer]
    [NoOneHaveBuff(typeof(DecoyWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(DecoyWizard))]
    DecoyWizard,

    [OnlyPlayer]
    [CantBeAppliedIfAlreadyExists(typeof(TotemOfUndying))]
    TotemOfUndying,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(TimeStopWizard))]
    TimeStopWizard,

    WeaponWithRandomPowerup,

    FlamyKatana,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(PoisonBullets))]
    PoisonBullets,

    [OnlyPlayer]
    [AlivePlayersMoreThanOne]
    [Weight(0.5f / 2)]
    [AnyPlayerHaveAnyWeapon(true)]
    RemoveWeaponsExceptPlayer,

    [Weight(0.5f / 2)]
    [AnyPlayerHaveAnyWeapon]
    RemoveWeapons,

    [OnlyPlayer]
    [CantBeAppliedIfAlreadyExists(typeof(RestoreWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(RestoreWizard))]
    RestoreWizard,

    [OnlyPlayer]
    [CantBeAppliedIfAlreadyExists(typeof(StealWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(StealWizard))]
    StealWizard,

    FunWeapon,

    [OnlyPlayer]
    [NoOneHaveBuff(typeof(TimeRevertWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(TimeRevertWizard))]
    TimeRevertWizard
}