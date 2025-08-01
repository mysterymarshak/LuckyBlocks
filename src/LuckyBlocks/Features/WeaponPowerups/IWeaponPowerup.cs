using System;
using LuckyBlocks.Data.Weapons;

namespace LuckyBlocks.Features.WeaponPowerups;

internal interface IWeaponPowerup<out T> where T : Weapon
{
    string Name { get; }
    bool IsHidden { get; }
    T Weapon { get; }
    IWeaponPowerup<T> Clone(Weapon copiedWeapon);
    void Run();
    bool IsCompatibleWith(Type otherPowerupType);
    void MoveToWeapon(Weapon otherWeapon);
    void Dispose();
}