using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Mined;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class LegendaryWeaponLoot : PowerUppedWeaponBase
{
    public override string Name => "Legendary weapon";
    public override Item Item => Item.LegendaryWeapon;

    protected override WeaponItem WeaponItem { get; }
    protected override WeaponItemType WeaponItemType => WeaponItem.GetWeaponItemType();

    private const double MinedChance = 0.1;

    private static readonly List<WeaponItem> LegendaryWeapons =
    [
        WeaponItem.BAZOOKA,
        WeaponItem.GRENADE_LAUNCHER,
        WeaponItem.SNIPER,
        WeaponItem.M60,
        WeaponItem.CHAINSAW,
        WeaponItem.MAGNUM,
        WeaponItem.FLAREGUN
    ];

    private readonly IPowerupFactory _powerupFactory;
    private readonly bool _isMined;

    public LegendaryWeaponLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
        WeaponItem = LegendaryWeapons.GetRandomElement();
        _isMined = SharedRandom.Instance.NextDouble() <= MinedChance;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        if (_isMined)
        {
            yield return _powerupFactory.CreatePowerup(weapon,
                WeaponItemType == WeaponItemType.Melee ? typeof(MinedMelee) : typeof(MinedFirearm));
        }
    }
}