using LuckyBlocks.Data;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface ILimitedAmmoPowerup<out T> : IUsablePowerup<T> where T : Weapon
{
    int MaxAmmo { get; }
}