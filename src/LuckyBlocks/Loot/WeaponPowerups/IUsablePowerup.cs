using LuckyBlocks.Data;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IUsablePowerup<out T> : IWeaponPowerup<T> where T : Weapon
{
    int UsesCount { get; }
    int UsesLeft { get; }
    void AddUses(int usesCount);
}