using System;
using LuckyBlocks.Data.Weapons;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IWeaponPowerup<out T> where T : Weapon
{
    string Name { get; }
    T Weapon { get; }
    IWeaponPowerup<T> Clone(Weapon copiedWeapon);
    void Run();
    bool IsCompatibleWith(Type otherPowerupType);
    void MoveToWeapon(Weapon otherWeapon);
    void Dispose();
}