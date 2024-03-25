using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.InfiniteBouncing;

internal class GrenadeLauncherWithInfiniteBouncing : WeaponWithInfiniteBouncingBase
{
    public override Item Item => Item.GrenadeLauncherWithInfiniteBouncing;
    public override string Name => "Grenade launcher with infinite bouncing";

    protected override WeaponItem WeaponItem => WeaponItem.GRENADE_LAUNCHER;
    protected override WeaponItemType WeaponItemType => WeaponItemType.Rifle;

    public GrenadeLauncherWithInfiniteBouncing(Vector2 spawnPosition, LootConstructorArgs lootConstructorArgs) : base(spawnPosition, lootConstructorArgs)
    {
    }
}