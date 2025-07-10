using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Loot.Buffs.Durable;
using LuckyBlocks.Loot.Buffs.Wizards;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;

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
    [ModifiedModifiers(typeof(StrongMan), nameof(Buffs.Durable.StrongMan.ModifiedModifiers))]
    StrongMan,

    [BarrelExists]
    ExplodeRandomBarrel,

    Medkit,

    MedkitPoisoned,

    [LuckyBlockDropChanceCanBeIncreased]
    IncreaseSpawnChance,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(HighJumps), nameof(Buffs.Durable.HighJumps.ModifiedModifiers))]
    HighJumps,

    [AlivePlayersMoreThanOne]
    ShufflePositions,

    [AlivePlayersMoreThanOne]
    [AnyPlayerHaveAnyWeapon]
    ShuffleWeapons,

    StickyGrenades,

    BananaGrenades,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Vampirism), nameof(Buffs.Durable.Vampirism.ModifiedModifiers))]
    Vampirism,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(WindWizard))]
    WindWizard,

    IgniteRandomPlayer,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Shield), nameof(Buffs.Durable.Shield.ModifiedModifiers))]
    Shield,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Hulk), nameof(Buffs.Durable.Hulk.ModifiedModifiers))]
    Hulk,

    [OnlyPlayer]
    [ModifiedModifiers(typeof(Dwarf), nameof(Buffs.Durable.Dwarf.ModifiedModifiers))]
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
    [AlivePlayersMoreThanOne]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(FreezeBullets))]
    FreezeBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithSomePowerups(SourcePowerup = typeof(PushBullets))]
    PushBullets,

    [OnlyPlayer]
    [AlivePlayersMoreThanOne]
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
    [Weight(0.5f / 2)]
    [AnyPlayerHaveAnyWeapon(true)]
    [AlivePlayersMoreThanOne]
    RemoveWeaponsExceptPlayer,

    [Weight(0.5f / 2)]
    [AnyPlayerHaveAnyWeapon]
    [AlivePlayersMoreThanOne]
    RemoveWeapons,

    [OnlyPlayer]
    [CantBeAppliedIfAlreadyExists(typeof(RestoreWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(RestoreWizard))]
    RestoreWizard,

    [OnlyPlayer]
    [CantBeAppliedIfAlreadyExists(typeof(StealWizard))]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(StealWizard))]
    StealWizard,

    FunWeapon
}