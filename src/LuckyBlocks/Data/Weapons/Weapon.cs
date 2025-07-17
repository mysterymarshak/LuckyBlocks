using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Reflection;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

[Inject]
internal record Weapon(WeaponItem WeaponItem, WeaponItemType WeaponItemType)
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    public static readonly Weapon Empty = new(WeaponItem.NONE, WeaponItemType.NONE);

    public event Action<Weapon, IPlayer>? PickUp;
    public event Action<IObjectWeaponItem?, Weapon>? Drop;
    public event Action<IObjectWeaponItem?, Weapon>? Throw;
    public event Action<Weapon>? Draw;
    public event Action<Weapon>? Hide;
    public event Action<Weapon>? Dispose;

    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE || _isDisposed ||
                             (IsDropped && ObjectId == 0 && !_copied);

    [MemberNotNullWhen(false, nameof(Owner))]
    public bool IsDropped => Owner?.IsValid() != true;

    public bool Copied => _copied;
    public virtual bool IsDrawn => !IsDropped && Owner!.CurrentWeaponDrawn == WeaponItemType;
    public IPlayer? Owner { get; private set; }
    public int ObjectId { get; private set; }

    public IEnumerable<IWeaponPowerup<Weapon>> Powerups
    {
        get => _powerups ?? Enumerable.Empty<IWeaponPowerup<Weapon>>();
        init => _powerups = value as List<IWeaponPowerup<Weapon>> ?? value.ToList();
    }

    private List<IWeaponPowerup<Weapon>>? _powerups;
    private bool _isDisposed;
    private bool _copied;

    public void SetOwner(IPlayer player)
    {
        Owner = player;
        ObjectId = 0;
        _copied = false;
    }

    public void SetObject(int objectId)
    {
        Owner = null;
        ObjectId = objectId;
        _copied = false;
    }

    public void SetCopied()
    {
        Owner = null;
        ObjectId = 0;
        _copied = true;
    }

    public void AddPowerup(IWeaponPowerup<Weapon> powerup)
    {
        _powerups ??= [];
        _powerups.Add(powerup);
    }

    public void RemovePowerup(IWeaponPowerup<Weapon> powerup)
    {
        _powerups!.Remove(powerup);
    }

    public virtual void RaiseEvent(WeaponEvent @event, params object?[] args)
    {
        if (_copied)
        {
            throw new InvalidOperationException("trying to raise event when weapon is copied");
        }

        Logger.Debug("Raised event {Event} for {WeaponItem} (owner: {Owner})", @event, WeaponItem, Owner?.Name);

        switch (@event)
        {
            case WeaponEvent.PickedUp:
                PickUp?.Invoke(this, (args[0] as IPlayer)!);
                break;
            case WeaponEvent.Dropped:
                Drop?.Invoke(args[0] as IObjectWeaponItem, this);
                break;
            case WeaponEvent.Thrown:
                Throw?.Invoke(args[0] as IObjectWeaponItem, this);
                break;
            case WeaponEvent.Drawn:
                Draw?.Invoke(this);
                break;
            case WeaponEvent.Hidden:
                Hide?.Invoke(this);
                break;
            case WeaponEvent.Disposed:
                Dispose?.Invoke(this);
                _isDisposed = true;
                break;
        }
    }

    public string GetFormattedName()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(WeaponItem);

        if (Powerups.Any())
        {
            stringBuilder.Append(" (");

            foreach (var powerup in Powerups)
            {
                stringBuilder.Append(powerup.Name);
                stringBuilder.Append(" & ");
            }

            stringBuilder.Remove(stringBuilder.Length - 3, 3);
            stringBuilder.Append(")");
        }

        return stringBuilder.ToString();
    }
}