using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot.Buffs;
using LuckyBlocks.Loot.Buffs.Wizards;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class RestoreMagic : NonAreaMagicBase
{
    public override string Name => "Restore magic";

    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IBuffsService _buffsService;
    private readonly ILogger _logger;

    private WizardState? _state;

    public RestoreMagic(Player wizard, BuffConstructorArgs args) : base(wizard, args)
    {
        _weaponPowerupsService = args.WeaponPowerupsService;
        _buffsService = args.BuffsService;
        _logger = args.Logger;
    }

    public override void Cast()
    {
        if (_state is null)
        {
            SaveState();
        }
        else
        {
            RestoreState();
        }
    }

    private void SaveState()
    {
        var wizardInstance = Wizard.Instance!;
        var position = wizardInstance.GetWorldPosition();
        var strengthBoostTime = wizardInstance.GetStrengthBoostTime();
        var speedBoostTime = wizardInstance.GetSpeedBoostTime();
        var health = wizardInstance.GetHealth();
        var isBurning = wizardInstance.IsBurning;
        var buffs = Wizard.Buffs
            .Where(x => x is ICloneableBuff<IBuff> and not RestoreWizard)
            .Cast<ICloneableBuff<IBuff>>()
            .Select(x => x.Clone())
            .ToList();
        var weaponsData = _weaponPowerupsService.CreateWeaponsDataCopy(Wizard);

#if DEBUG
        _logger.Debug("Saved state for '{Player}' Weapons: '{WeaponsCount}' Powerups: '{Powerups}' Buffs: '{BuffsCount} | {Buffs}'",
            Wizard.Name,
            weaponsData.Count(),
            weaponsData.Select(x => string.Join(", ", x.Powerups.Select(y => $"{y.Weapon.WeaponItem}: {y.Name}"))).ToList(),
            buffs.Count,
            buffs.Select(x => x.Name).ToList());
#endif

        _state = new WizardState(position, strengthBoostTime, speedBoostTime, health, isBurning, buffs, weaponsData);
    }

    private void RestoreState()
    {
        var wizardInstance = Wizard.Instance!;
        var (position, strengthBoostTime, speedBoostTime, health, isBurning, buffs, weaponsData) = _state!;

        wizardInstance.SetWorldPosition(position);
        wizardInstance.SetStrengthBoostTime(strengthBoostTime);
        wizardInstance.SetSpeedBoostTime(speedBoostTime);
        wizardInstance.SetHealth(health);

        if (isBurning)
        {
            wizardInstance.SetMaxFire();
        }

        Wizard.RemoveAllBuffs();
        foreach (var buff in buffs)
        {
            _buffsService.TryAddBuff(buff, Wizard);
        }

        _weaponPowerupsService.RestoreWeaponsDataFromCopy(Wizard, weaponsData);
    }

    private record WizardState(
        Vector2 Position,
        float StrengthBoostTime,
        float SpeedBoostTime,
        float Health,
        bool IsBurning,
        IEnumerable<IBuff> Buffs,
        WeaponsData WeaponsData);
}