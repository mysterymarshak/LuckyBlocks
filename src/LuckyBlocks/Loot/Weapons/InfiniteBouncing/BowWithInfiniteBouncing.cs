using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.InfiniteBouncing;

internal class BowWithInfiniteBouncing : WeaponWithInfiniteBouncingBase
{
    public override Item Item => Item.BowWithInfiniteBouncing;
    public override string Name => "Bow with infinite bouncing";

    protected override WeaponItem WeaponItem => WeaponItem.BOW;
    protected override WeaponItemType WeaponItemType => WeaponItemType.Rifle;

    public BowWithInfiniteBouncing(Vector2 spawnPosition, LootConstructorArgs lootConstructorArgs) : base(spawnPosition, lootConstructorArgs)
    {
    }
}