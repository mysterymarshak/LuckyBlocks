using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Loot.Buffs;
using LuckyBlocks.Loot.Buffs.Durable;
using LuckyBlocks.Loot.Buffs.Instant;
using LuckyBlocks.Loot.Buffs.Wizards;
using LuckyBlocks.Loot.Items;
using LuckyBlocks.Loot.Other;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using LuckyBlocks.Loot.Weapons;
using LuckyBlocks.Loot.Weapons.Grenades;
using LuckyBlocks.Loot.Weapons.InfiniteBouncing;
using LuckyBlocks.Loot.Weapons.LegendaryWeapon;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot;

internal interface ILootFactory
{
    OneOf<ILoot, Error<string>> CreateLoot(LuckyBlockBrokenArgs brokenArgs, Item item);
}

internal class LootFactory : ILootFactory
{
    private readonly IIdentityService _identityService;
    private readonly LootConstructorArgs _lootConstructorArgs;
    private readonly IBuffFactory _buffFactory;
    private readonly IBuffWrapper _buffWrapper;

    public LootFactory(IIdentityService identityService, LootConstructorArgs lootConstructorArgs,
        IBuffFactory buffFactory, IBuffWrapper buffWrapper) =>
        (_identityService, _lootConstructorArgs, _buffFactory, _buffWrapper) =
        (identityService, lootConstructorArgs, buffFactory, buffWrapper);

    public OneOf<ILoot, Error<string>> CreateLoot(LuckyBlockBrokenArgs brokenArgs, Item item)
    {
        var isPlayerBuff = EnumUtils.AttributeExist<OnlyPlayerAttribute, Item>(item);
        if (isPlayerBuff)
        {
            var getPlayerResult = _identityService.GetPlayerById(brokenArgs.PlayerId);
            if (getPlayerResult.TryPickT1(out _, out var player))
                return new Error<string>(
                    $"Attempt to create player loot for unknown player with id '{brokenArgs.PlayerId}'");

            var getPlayerBuffResult = GetPlayerLoot(player, item);
            return getPlayerBuffResult.Match(
                OneOf<ILoot, Error<string>>.FromT0,
                _ => new Error<string>($"Attempt to create invalid player loot '{item}'"));
        }

        var getNonPlayerLootResult = GetNonPlayerLoot(brokenArgs.Position, item);
        return getNonPlayerLootResult.Match(
            OneOf<ILoot, Error<string>>.FromT0,
            _ => new Error<string>($"Attempt to create invalid non-player loot '{item}'"));
    }

    private OneOf<ILoot, ArgumentOutOfRangeException> GetPlayerLoot(Player player, Item item) => item switch
    {
        Item.AimBullets => new WeaponPowerupWrapper(typeof(AimBullets), item, player.Instance, _lootConstructorArgs),
        Item.PushBullets => new WeaponPowerupWrapper(typeof(PushBullets), item, player.Instance, _lootConstructorArgs),
        Item.FreezeBullets => new WeaponPowerupWrapper(typeof(FreezeBullets), item, player.Instance,
            _lootConstructorArgs),
        Item.TripleRicochetBullets => new WeaponPowerupWrapper(typeof(TripleRicochetBullets), item, player.Instance,
            _lootConstructorArgs),
        Item.ExplosiveBullets => new WeaponPowerupWrapper(typeof(ExplosiveBullets), item, player.Instance,
            _lootConstructorArgs),
        Item.InfiniteRicochetBullets => new WeaponPowerupWrapper(typeof(InfiniteRicochetBullets), item, player.Instance,
            _lootConstructorArgs),
        Item.PoisonBullets => new WeaponPowerupWrapper(typeof(PoisonBullets), item, player.Instance,
            _lootConstructorArgs),
        Item.Vampirism => CreateWrappedBuff<Vampirism>(player, Item.Vampirism),
        Item.StrongMan => CreateWrappedBuff<StrongMan>(player, Item.StrongMan),
        Item.Shield => CreateWrappedBuff<Shield>(player, Item.Shield),
        Item.HighJumps => CreateWrappedBuff<HighJumps>(player, Item.HighJumps),
        Item.Hulk => CreateWrappedBuff<Hulk>(player, Item.Hulk),
        Item.Dwarf => CreateWrappedBuff<Dwarf>(player, Item.Dwarf),
        Item.Freeze => CreateWrappedBuff<Freeze>(player, Item.Freeze),
        Item.FullHp => CreateWrappedBuff<FullHp>(player, Item.FullHp),
        Item.FireWizard => CreateWrappedBuff<FireWizard>(player, Item.FireWizard),
        Item.ElectricWizard => CreateWrappedBuff<ElectricWizard>(player, Item.ElectricWizard),
        Item.DecoyWizard => CreateWrappedBuff<DecoyWizard>(player, Item.DecoyWizard),
        Item.TotemOfUndying => CreateWrappedBuff<TotemOfUndying>(player, Item.TotemOfUndying),
        Item.WindWizard => CreateWrappedBuff<WindWizard>(player, Item.WindWizard),
        Item.TimeStopWizard => CreateWrappedBuff<TimeStopWizard>(player, Item.TimeStopWizard),
        _ => new ArgumentOutOfRangeException(nameof(item))
    };

    private OneOf<ILoot, ArgumentOutOfRangeException> CreateWrappedBuff<T>(Player player, Item item)
        where T : class, IBuff
    {
        var buff = _buffFactory.CreateBuff<T>(player);
        return OneOf<ILoot, ArgumentOutOfRangeException>.FromT0(_buffWrapper.Wrap(item, buff, player));
    }

    private OneOf<ILoot, ArgumentOutOfRangeException> GetNonPlayerLoot(Vector2 position, Item item) => item switch
    {
        Item.LegendaryWeapon => new LegendaryWeaponWrapper(position, _lootConstructorArgs),
        Item.ShuffleWeapons => new ShuffleWeapons(_lootConstructorArgs),
        Item.ShufflePositions => new ShufflePositions(_lootConstructorArgs),
        Item.RespawnRandomPlayer => new RespawnRandomPlayer(_lootConstructorArgs),
        Item.IncreaseSpawnChance => new IncreaseSpawnChance(_lootConstructorArgs),
        Item.IgniteRandomPlayer => new IgniteRandomPlayer(_lootConstructorArgs),
        Item.Explosion => new Explosion(position, _lootConstructorArgs),
        Item.ExplodeRandomBarrel => new ExplodeRandomBarrel(_lootConstructorArgs),
        Item.BloodyBath => new BloodyBath(_lootConstructorArgs),
        Item.Medkit => new Medkit(position, _lootConstructorArgs),
        Item.MedkitPoisoned => new MedkitPoisoned(position, _lootConstructorArgs),
        Item.StickyGrenades => new StickyGrenadesLoot(position, _lootConstructorArgs),
        Item.BananaGrenades => new BananaGrenadesLoot(position, _lootConstructorArgs),
        Item.BowWithPushingAndInfiniteBouncing => new BowWithPushingAndInfiniteBouncing(position, _lootConstructorArgs),
        Item.GrenadeLauncherWithInfiniteBouncing => new GrenadeLauncherWithInfiniteBouncing(position,
            _lootConstructorArgs),
        Item.WeaponWithRandomPowerup => new WeaponWithRandomPowerup(position, _lootConstructorArgs),
        Item.FlamyKatana => new FlamyKatanaLoot(position, _lootConstructorArgs),
        _ => new ArgumentOutOfRangeException(nameof(item))
    };
}