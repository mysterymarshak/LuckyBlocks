using System;
using LuckyBlocks.Data.Weapons.Unsafe;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal record Melee(WeaponItem WeaponItem, WeaponItemType WeaponItemType, float CurrentDurability)
    : Weapon(WeaponItem, WeaponItemType)
{
    public event Action<PlayerMeleeHitArg>? MeleeHit;

    public override bool IsDrawn => !IsDropped && Owner!.CurrentWeaponDrawn == WeaponItemType &&
                                    Owner.CurrentMeleeMakeshiftWeapon.WeaponItem == WeaponItem.NONE;

    public float CurrentDurability { get; protected set; } = CurrentDurability;
    public float MaxDurability => 1f;

    public void Update(in UnsafeMelee newData)
    {
        CurrentDurability = newData.CurrentDurability;
    }

    public override void RaiseEvent(WeaponEvent @event, params object?[] args)
    {
        base.RaiseEvent(@event, args);

        if (@event == WeaponEvent.MeleeHit)
        {
            MeleeHit?.Invoke((PlayerMeleeHitArg)args[0]!);
        }
    }

    public void SetDurability(float durability)
    {
        if (IsDropped)
        {
            throw new InvalidOperationException();
        }

        durability = MathHelper.Clamp(durability, 0f, MaxDurability);
        CurrentDurability = durability;

        if (this is MeleeTemp)
        {
            Owner.SetCurrentMeleeMakeshiftDurability(durability);
        }
        else
        {
            Owner.SetCurrentMeleeDurability(durability);
        }
    }
}