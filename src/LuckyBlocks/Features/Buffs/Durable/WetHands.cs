using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;
using System.Collections.Generic;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Profiles;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class WetHands : DurableRepressibleByImmunityFlagsBuffBase
{
    public override string Name => "Wet hands";
    public override TimeSpan Duration => TimeSpan.FromSeconds(20);
    public override ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToWater;
    public override Color BuffColor => ExtendedColors.Water;
    public override Color ChatColor => ExtendedColors.LightWater;

    private const double SlippingChance = 0.1;

    private readonly IProfilesService _profileService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly BuffConstructorArgs _args;
    private readonly List<Weapon> _hookedWeapons = [];

    public WetHands(Player player, BuffConstructorArgs args) : base(player, args)
    {
        _profileService = args.ProfilesService;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new WetHands(Player, _args);
    }

    protected override void OnRunInternal()
    {
        HookExistingWeaponsEvents();
        ExtendedEvents.HookOnWeaponAdded(PlayerInstance!, OnWeaponAdded, EventHookMode.LessPrioritized);

        ShowWetDialogue("MY HANDS ARE WET");
        _profileService.RequestProfileChanging<WetHands>(Player);
        _effectsPlayer.PlayEffect(EffectName.WaterSplash, PlayerInstance!.GetWorldPosition() - new Vector2(0, 5));
    }

    protected override void OnApplyAgainInternal()
    {
        ShowWetDialogue("WET AGAIN :(");
        ShowChatMessage($"Your hands are wet for {TimeLeft.TotalSeconds}s");
    }

    protected override void OnFinishInternal()
    {
        _profileService.RequestProfileRestoring<WetHands>(Player);

        foreach (var weapon in _hookedWeapons)
        {
            UnHookEvents(weapon);
        }
    }

    private void HookExistingWeaponsEvents()
    {
        foreach (var weapon in Player.WeaponsData)
        {
            HookEvents(weapon);
        }
    }

    private void OnWeaponAdded(Event<PlayerWeaponAddedArg> @event)
    {
        var args = @event.Args;
        var weaponsData = Player.WeaponsData;
        var weapon = weaponsData.GetWeaponByType(args.WeaponItemType,
            PlayerInstance!.CurrentMeleeMakeshiftWeapon.WeaponItem != WeaponItem.NONE);

        if (weapon.IsInvalid)
            return;

        _hookedWeapons.Add(weapon);
        HookEvents(weapon);

        if (!RollForSlip())
            return;

        Disarm(weapon.WeaponItemType);
    }

    private void OnWeaponRemoved(Weapon weapon)
    {
        _hookedWeapons.Remove(weapon);
        UnHookEvents(weapon);
    }

    private void HookEvents(Weapon weapon)
    {
        weapon.Draw += OnDrawn;

        if (weapon is Firearm firearm)
        {
            firearm.Fire += OnFire;
        }
        else if (weapon is Melee melee)
        {
            melee.MeleeHit += OnMeleeHit;
        }
        else if (weapon is Throwable throwable)
        {
            throwable.Activate += OnThrowableActivate;
        }

        weapon.Dispose += OnWeaponRemoved;
        weapon.Throw += OnWeaponThrown;
        weapon.Drop += OnWeaponDropped;
    }

    private void UnHookEvents(Weapon weapon)
    {
        weapon.Draw -= OnDrawn;

        if (weapon is Firearm firearm)
        {
            firearm.Fire -= OnFire;
        }
        else if (weapon is Melee melee)
        {
            melee.MeleeHit -= OnMeleeHit;
        }
        else if (weapon is Throwable throwable)
        {
            throwable.Activate -= OnThrowableActivate;
        }

        weapon.Dispose -= OnWeaponRemoved;
        weapon.Throw -= OnWeaponThrown;
        weapon.Drop -= OnWeaponDropped;
    }

    private void OnDrawn(Weapon weapon) => OnWeaponUse(weapon);

    private void OnFire(Weapon weapon, IPlayer playerInstance, IEnumerable<IProjectile> projectilesEnumerable) =>
        OnWeaponUse(weapon);

    private void OnMeleeHit(Weapon weapon, PlayerMeleeHitArg args) => OnWeaponUse(weapon);
    private void OnThrowableActivate(Weapon weapon) => OnWeaponUse(weapon);
    private void OnWeaponThrown(IObjectWeaponItem? objectWeaponItem, Weapon weapon) => OnWeaponRemoved(weapon);
    private void OnWeaponDropped(IObjectWeaponItem? objectWeaponItem, Weapon weapon) => OnWeaponRemoved(weapon);

    private void OnWeaponUse(Weapon weapon)
    {
        if (!RollForSlip())
            return;

        if (weapon is Throwable { IsActive: true })
        {
            PlayerInstance!.DisarmActiveThrowable();
        }
        else
        {
            Disarm(weapon.WeaponItemType);
        }
    }

    private void ShowWetDialogue(string message)
    {
        ShowDialogue(message, TimeSpan.FromMilliseconds(2500), BuffColor);
    }

    private void Disarm(WeaponItemType weaponItemType)
    {
        var disarmedWeapon =
            PlayerInstance!.Disarm(weaponItemType, new Vector2(1 * PlayerInstance.GetFaceDirection(), -0.5f));
        if (disarmedWeapon is not null)
        {
            var position = disarmedWeapon.GetWorldPosition();
            _effectsPlayer.PlayEffect(EffectName.Block, position);
            _effectsPlayer.PlaySoundEffect("Throw", position);
        }
    }

    private bool RollForSlip()
    {
        return SharedRandom.Instance.NextDouble() <= SlippingChance;
    }
}