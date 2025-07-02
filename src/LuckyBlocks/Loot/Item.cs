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

    BowWithPushingAndInfiniteBouncing,

    GrenadeLauncherWithInfiniteBouncing,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(FireWizard))]
    FireWizard,

    [OnlyPlayer]
    [PlayerIsNotOtherWizard(SourceWizard = typeof(ElectricWizard))]
    ElectricWizard,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(typeof(TripleRicochetBullets), typeof(InfiniteRicochetBullets),
        SourcePowerup = typeof(ExplosiveBullets))]
    ExplosiveBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(typeof(ExplosiveBullets), typeof(FreezeBullets), typeof(InfiniteRicochetBullets),
        typeof(AimBullets), typeof(PushBullets),
        SourcePowerup = typeof(TripleRicochetBullets))]
    TripleRicochetBullets,

    [OnlyPlayer]
    [AlivePlayersMoreThanOne]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(typeof(TripleRicochetBullets), typeof(PushBullets),
        SourcePowerup = typeof(FreezeBullets))]
    FreezeBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(typeof(TripleRicochetBullets), typeof(FreezeBullets),
        SourcePowerup = typeof(PushBullets))]
    PushBullets,

    [OnlyPlayer]
    [AlivePlayersMoreThanOne]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(typeof(TripleRicochetBullets), SourcePowerup = typeof(AimBullets))]
    AimBullets,

    [OnlyPlayer]
    [PlayerHasAnyFirearm]
    [IncompatibleWithPowerups(typeof(ExplosiveBullets), typeof(TripleRicochetBullets),
        SourcePowerup = typeof(InfiniteRicochetBullets))]
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

    WeaponWithRandomPowerup
}