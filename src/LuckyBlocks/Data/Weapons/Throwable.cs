using System;
using LuckyBlocks.Data.Weapons.Unsafe;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal record Throwable(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxAmmo,
    bool IsActive)
    : Weapon(WeaponItem, WeaponItemType)
{
    public event Action<IPlayer?, IObject?, Throwable?>? GrenadeThrow;
    public event Action? Activate;

    public int CurrentAmmo { get; protected set; } = CurrentAmmo;
    public bool IsActive { get; protected set; } = IsActive;

    public void Update(in UnsafeThrowable newData, bool isActive)
    {
        CurrentAmmo = newData.CurrentAmmo;
        IsActive = isActive;
    }

    public override void RaiseEvent(WeaponEvent @event, params object?[] args)
    {
        base.RaiseEvent(@event, args);

        switch (@event)
        {
            case WeaponEvent.GrenadeThrown:
                GrenadeThrow?.Invoke(args[0] as IPlayer, args[1] as IObject, args[2] as Throwable);
                break;
            case WeaponEvent.Activated:
                Activate?.Invoke();
                break;
        }
    }
}