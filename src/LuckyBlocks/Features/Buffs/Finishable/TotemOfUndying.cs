using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Finishable;

internal class TotemOfUndying : FinishableBuffBase, ICloneableBuff<IFinishableBuff>
{
    public override string Name => "Totem Of Undying";
    public override Color BuffColor => ExtendedColors.TotemOfUndying;

    private readonly BuffConstructorArgs _args;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IRespawner _respawner;
    private readonly ILogger _logger;

    private WeaponsData? _weaponsDataCopy;

    public TotemOfUndying(Player player, BuffConstructorArgs args) : base(player, args)
    {
        _args = args;
        _weaponPowerupsService = args.WeaponPowerupsService;
        _respawner = args.Respawner;
        _logger = args.Logger;
    }

    public IFinishableBuff Clone(Player? player = null)
    {
        return new TotemOfUndying(player ?? Player, _args);
    }

    public override void Run()
    {
        ExtendedEvents.HookOnDead(PlayerInstance!, OnDead, EventHookMode.Default);
        ExtendedEvents.HookOnDamage(PlayerInstance!, OnDamage, EventHookMode.GlobalThisPre);

        ShowTotemDialogue("TOTEM OF UNDYING", true);

        if (Player.IsFake())
        {
            InternalFinish();
        }
    }

    private bool OnDamage(Event<PlayerDamageArgs> @event)
    {
        var args = @event.Args;

        if (!Player.IsValid())
        {
            InternalFinish();
            return false;
        }

        if (args.Damage < PlayerInstance!.GetHealth())
        {
            return false;
        }

        var maxHealth = PlayerInstance.GetMaxHealth();
        PlayerInstance.SetHealth(maxHealth);

        _logger.Debug("{PlayerName} respawned from totem", Player.Name);

        OnRespawned();
        return true;
    }

    private void OnDead(Event<PlayerDeathArgs> @event)
    {
        var args = @event.Args;
        if (args.Removed || !Player.IsValid())
        {
            InternalFinish();
            return;
        }

        _weaponsDataCopy = _weaponPowerupsService.CreateWeaponsDataCopy(Player);

        CloseDialogue();

        var user = Player.User;
        var profile = Player.Profile;
        _respawner.RespawnPlayer(user, profile);

        ScheduleWeaponsDataRestoring();
        OnRespawned();
    }

    private void ScheduleWeaponsDataRestoring()
    {
        Awaiter.Start(delegate { _weaponPowerupsService.RestoreWeaponsDataFromCopy(Player, _weaponsDataCopy!); }, 1);
    }

    private void OnRespawned()
    {
        ShowTotemDialogue("TOTEM SAVED YOU", true);
        InternalFinish();
    }

    private void ShowTotemDialogue(string message, bool ignoreFinish = false)
    {
        ShowDialogue(message, TimeSpan.FromSeconds(3), BuffColor, ignoreFinish: ignoreFinish);
    }
}