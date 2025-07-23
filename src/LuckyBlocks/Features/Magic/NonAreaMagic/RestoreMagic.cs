using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Wizards;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.WeaponPowerups;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class RestoreMagic : NonAreaMagicBase
{
    public event Action? StateSave;
    public event Action? StateRestore;

    public override string Name => "Restore magic";
    public override bool ShouldCastOnRestore => false;

    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IBuffsService _buffsService;
    private readonly MagicConstructorArgs _args;
    private readonly ILogger _logger;

    private WizardState? _state;

    public RestoreMagic(Player wizard, MagicConstructorArgs args) : base(wizard, args)
    {
        _weaponPowerupsService = args.WeaponPowerupsService;
        _buffsService = args.BuffsService;
        _logger = args.Logger;
        _args = args;
    }

    public override void Cast()
    {
        if (_state is null)
        {
            SaveState();
            StateSave?.Invoke();
        }
        else
        {
            RestoreState();
            StateRestore?.Invoke();
            ExternalFinish();
        }
    }

    public override MagicBase Copy()
    {
        return new RestoreMagic(Wizard, _args) { _state = _state };
    }

    private void SaveState()
    {
        var wizardInstance = Wizard.Instance!;
        var position = wizardInstance.GetWorldPosition();
        var strengthBoostTime = wizardInstance.GetStrengthBoostTime();
        var speedBoostTime = wizardInstance.GetSpeedBoostTime();
        var health = wizardInstance.GetHealth();
        var isBurning = wizardInstance.IsBurning;
        var buffs = Wizard.CloneBuffs([typeof(RestoreWizard)]);
        var weaponsData = _weaponPowerupsService.CreateWeaponsDataCopy(Wizard);

#if DEBUG
        _logger.Debug(
            "Saved state for '{Player}' Weapons: '{WeaponsCount}' Powerups: '{Powerups}' Buffs: '{BuffsCount} | {Buffs}'",
            Wizard.Name,
            weaponsData.Count(),
            weaponsData.Select(x => string.Join(", ", x.Powerups.Select(y => $"{y.Weapon.WeaponItem}: {y.Name}")))
                .ToList(),
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