using System;
using LuckyBlocks.Data;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IWeaponPowerup<out T> where T : Weapon
{
    string Name { get; }
    T Weapon { get; }
    void Run();
    void Dispose();
    bool IsCompatibleWith(Type otherPowerupType);
    void MoveToWeapon(Weapon otherWeapon);
}