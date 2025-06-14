using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.InfiniteBouncing;

internal class BowWithPushingAndInfiniteBouncing : WeaponWithInfiniteBouncingBase
{
    public override Item Item => Item.BowWithPushingAndInfiniteBouncing;
    public override string Name => "Bow with pushing and infinite bouncing";

    protected override WeaponItem WeaponItem => WeaponItem.BOW;
    protected override WeaponItemType WeaponItemType => WeaponItemType.Rifle;

    private readonly IPowerupFactory _powerupFactory;
    
    public BowWithPushingAndInfiniteBouncing(Vector2 spawnPosition, LootConstructorArgs lootConstructorArgs) : base(spawnPosition, lootConstructorArgs)
    {
        _powerupFactory = lootConstructorArgs.PowerupFactory;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, typeof(PushBullets));
        
        foreach (var powerup in base.GetPowerups(weapon))
        {
            yield return powerup;
        }
    }
}