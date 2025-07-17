using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Utils.Watchers;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedObjectWeaponItem : RevertedDynamicObject
{
    private readonly IWeaponsDataWatcher _weaponsDataWatcher;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IMediator _mediator;
    private readonly WeaponItem _weaponItem;
    private readonly float _despawnTime;
    private readonly bool _breakOnDrop;
    private readonly float _ammo;
    private readonly List<IWeaponPowerup<Weapon>> _powerups;
    private readonly bool _isMissile;

    public RevertedObjectWeaponItem(IObjectWeaponItem objectWeaponItem, IWeaponsDataWatcher weaponsDataWatcher,
        IWeaponPowerupsService weaponPowerupsService, IMediator mediator) : base(objectWeaponItem)
    {
        _weaponsDataWatcher = weaponsDataWatcher;
        _weaponPowerupsService = weaponPowerupsService;
        _mediator = mediator;
        _weaponItem = objectWeaponItem.WeaponItem;
        _despawnTime = objectWeaponItem.DespawnTime;
        _breakOnDrop = objectWeaponItem.BreakOnDrop;
        _ammo = objectWeaponItem.GetCurrentAmmo();
        _isMissile = objectWeaponItem.IsMissile;
        var weapon = _weaponsDataWatcher.RegisterWeapon(objectWeaponItem);
        _powerups = _weaponPowerupsService.CreateWeaponPowerupsCopy(weapon).ToList();
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        var objectWeaponItem = (IObjectWeaponItem)Object;

        if (objectWeaponItem.DespawnTime != _despawnTime)
        {
            objectWeaponItem.SetDespawnTime((int)_despawnTime);
        }

        if (objectWeaponItem.BreakOnDrop != _breakOnDrop)
        {
            objectWeaponItem.SetBreakOnDrop(_breakOnDrop);
        }

        if (objectWeaponItem.IsMissile != _isMissile)
        {
            objectWeaponItem.TrackAsMissile(_isMissile);
        }

        var weapon = _weaponsDataWatcher.RegisterWeapon(objectWeaponItem);
        foreach (var powerup in weapon.Powerups)
        {
            _weaponPowerupsService.RemovePowerup(powerup, weapon);
        }

        _weaponPowerupsService.ConcatPowerups(weapon, _powerups);

        if (objectWeaponItem.GetCurrentAmmo() != _ammo)
        {
            var changedAmmoPowerup = new ChangedAmmo(weapon, _mediator, game);
            changedAmmoPowerup.SetAmmo((int)_ammo);
            _weaponPowerupsService.AddWeaponPowerup(changedAmmoPowerup, weapon);
        }

        // public override float GetCurrentAmmo()
        // {
        //     if (IsRemoved)
        //     {
        //         return 0f;
        //     }
        //     return WeaponItemType switch
        //     {
        //         SFDGameScriptInterface.WeaponItemType.Handgun => HandgunWeapon.TotalAmmo, 
        //         SFDGameScriptInterface.WeaponItemType.Rifle => RifleWeapon.TotalAmmo, 
        //         SFDGameScriptInterface.WeaponItemType.Melee => MeleeWeapon.CurrentValue, 
        //         SFDGameScriptInterface.WeaponItemType.Thrown => ThrownWeapon.CurrentAmmo, 
        //         _ => 0f, 
        //     };
        // }
        // from sfd source code
    }

    protected override IObject? Respawn(IGame game)
    {
        var objectWeaponItem = game.SpawnWeaponItem(_weaponItem, WorldPosition, true, _despawnTime);
        objectWeaponItem.SetLinearVelocity(LinearVelocity);
        objectWeaponItem.SetAngularVelocity(AngularVelocity);
        objectWeaponItem.SetAngle(Angle);
        objectWeaponItem.SetFaceDirection(Direction);

        return objectWeaponItem;
    }
}