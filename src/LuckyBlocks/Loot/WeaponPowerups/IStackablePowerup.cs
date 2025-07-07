using LuckyBlocks.Data.Weapons;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IStackablePowerup<out T> : IWeaponPowerup<T> where T : Weapon
{
    void Stack(IStackablePowerup<Weapon> powerup);
}