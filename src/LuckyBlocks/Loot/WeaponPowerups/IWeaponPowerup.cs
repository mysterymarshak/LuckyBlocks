using System;
using LuckyBlocks.Data.Weapons;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IWeaponPowerup<out T> where T : Weapon
{
    string Name { get; }
    T Weapon { get; }
    void Run();
    IWeaponPowerup<T> Clone(Weapon copiedWeapon);
    bool IsCompatibleWith(Type otherPowerupType);
    void MoveToWeapon(Weapon otherWeapon);
    void Dispose();
}