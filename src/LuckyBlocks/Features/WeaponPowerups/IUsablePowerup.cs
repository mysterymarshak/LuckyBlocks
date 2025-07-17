using LuckyBlocks.Data.Weapons;

namespace LuckyBlocks.Features.WeaponPowerups;

internal interface IUsablePowerup<out T> : IStackablePowerup<T> where T : Weapon
{
    int UsesCount { get; }
    int UsesLeft { get; }
}