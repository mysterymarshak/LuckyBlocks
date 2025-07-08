using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Weapons.Unsafe;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal record Firearm(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxTotalAmmo,
    int MagSize,
    int CurrentSpareMags,
    int MaxSpareMags,
    bool IsLazerEquipped,
    ProjectilePowerupData ProjectilePowerupData,
    ProjectileItem ProjectileItem)
    : Weapon(WeaponItem, WeaponItemType)
{
    public event Action<IPlayer?, IEnumerable<IProjectile>?>? Fire;
    public event Action<Weapon>? Reload;

    public int CurrentAmmo { get; protected set; } = CurrentAmmo;
    public int CurrentSpareMags { get; protected set; } = CurrentSpareMags;
    public bool IsLazerEquipped { get; protected set; } = IsLazerEquipped;
    public ProjectilePowerupData ProjectilePowerupData { get; protected set; } = ProjectilePowerupData;
    public int TotalAmmo => CurrentAmmo + (CurrentSpareMags * MagSize);

    public void Update(in UnsafeFirearm newData)
    {
        CurrentAmmo = newData.CurrentAmmo;
        CurrentSpareMags = newData.CurrentSpareMags;
        IsLazerEquipped = newData.IsLazerEquipped;
        ProjectilePowerupData = newData.ProjectilePowerupData;
    }

    public override void RaiseEvent(WeaponEvent @event, params object?[] args)
    {
        base.RaiseEvent(@event, args);

        switch (@event)
        {
            case WeaponEvent.Fired:
                Fire?.Invoke(args[0] as IPlayer, args[1] as IEnumerable<IProjectile>);
                break;
            case WeaponEvent.Reloaded:
                Reload?.Invoke(this);
                break;
        }
    }
}