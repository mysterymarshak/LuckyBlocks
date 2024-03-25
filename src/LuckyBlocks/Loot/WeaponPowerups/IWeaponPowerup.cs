using System.Collections.Generic;
using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IFirearmPowerup : IWeaponPowerup<Firearm>
{
    void OnFire(IPlayer player, IEnumerable<IProjectile> projectiles);
}

internal interface IThrowableItemPowerup<out T> : IWeaponPowerup<T> where T : Throwable
{
    void OnThrow(IPlayer player, IObject thrown);
}

internal interface IWeaponPowerup<out T> where T : Weapon
{
    string Name { get; }
    T? Weapon { get; }
    void OnRan(IPlayer player);
    void OnWeaponPickedUp(IPlayer player);
    void OnWeaponDropped(IPlayer player, IObjectWeaponItem? objectWeaponItem);
}