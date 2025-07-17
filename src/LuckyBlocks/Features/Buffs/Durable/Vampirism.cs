using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs.Instant;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class Vampirism : DurableBuffBase
{
    public static readonly SFDGameScriptInterface.PlayerModifiers ModifiedModifiers = new()
    {
        RunSpeedModifier = 1.2f,
        SprintSpeedModifier = 1.2f,
        ClimbingSpeed = 1.2f,
        EnergyRechargeModifier = EnergyRechargeModifier,
        EnergyConsumptionModifier = EnergyConsumptionModifier
    };

    public override string Name => "Vampirism";
    public override TimeSpan Duration => TimeSpan.FromSeconds(15);

    protected override Color BuffColor => ExtendedColors.ImperialRed;

    private const float BuffedEnergyRechargeModifier = 1.5f;
    private const float BuffedEnergyConsumptionModifier = 0.75f;
    private const float EnergyRechargeModifier = 1.01f;
    private const float EnergyConsumptionModifier = 0.99f;
    // needs to be != 1f because indicator for PlayerModifiersService

    private const int PoisonAmount = 25;
    private const int MedkitHealthEffect = 50;
    private const int PillsHealthEffect = 25;

    private readonly IBuffsService _buffsService;
    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly IGame _game;
    private readonly BuffConstructorArgs _args;
    private readonly RandomPeriodicTimer _bloodEffectTimer;
    private readonly Timer _energyRechargeBuffTimer;

    private SFDGameScriptInterface.PlayerModifiers? _playerModifiers;

    public Vampirism(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
    {
        _buffsService = args.BuffsService;
        _notificationService = args.NotificationService;
        _effectsPlayer = args.EffectsPlayer;
        _playerModifiersService = args.PlayerModifiersService;
        _game = args.Game;
        _args = args;
        _bloodEffectTimer = new RandomPeriodicTimer(TimeSpan.FromMilliseconds(600), TimeSpan.FromMilliseconds(1000),
            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop, OnBloodEffectCallback, ExtendedEvents);
        _energyRechargeBuffTimer = new Timer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier,
            OnRestoreEnergyModifier, ExtendedEvents);
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new Vampirism(Player, _args, TimeLeft);
    }

    protected override void OnRunInternal()
    {
        _playerModifiers = PlayerInstance!.GetModifiers();

        ExtendedEvents.HookOnDamage(OnDamage, EventHookMode.Default);
        ExtendedEvents.HookOnWeaponAdded(PlayerInstance!, OnWeaponAdded, EventHookMode.Default);

        EnableBuff();
        UpdateDialogue();
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue();
        ShowChatMessage($"You're blood sucker again for {TimeLeft.TotalSeconds}s");
    }

    protected override void OnFinishInternal()
    {
        DisableBuff();

        _bloodEffectTimer.Stop();
        _energyRechargeBuffTimer.Stop();
    }

    private void EnableBuff()
    {
        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
        _bloodEffectTimer.Start();
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
    }

    private void OnDamage(Event<IPlayer, PlayerDamageArgs> @event)
    {
        var (attackedPlayer, args, _) = @event;

        if (attackedPlayer == PlayerInstance || attackedPlayer?.IsValidUser() != true)
            return;

        switch (args.DamageType)
        {
            case PlayerDamageEventType.Melee when args.SourceID != PlayerInstance!.UniqueId:
                return;
            case PlayerDamageEventType.Projectile:
            {
                var projectile = _game.GetProjectile(args.SourceID);
                if (projectile.InitialOwnerPlayerID != PlayerInstance!.UniqueId)
                    return;

                break;
            }
        }

        OnAttack(args);
    }

    private void OnAttack(PlayerDamageArgs args)
    {
        var health = PlayerInstance!.GetHealth();
        var maxHealth = PlayerInstance.GetMaxHealth();
        var newHealth = Math.Min(maxHealth, health + args.Damage);
        var difference = newHealth - health;

        if (difference <= 0)
            return;

        PlayerInstance.SetHealth(newHealth);
        GiveEnergyBuff(PlayerInstance, difference);

        _notificationService.CreateTextNotification($"+{Math.Round(difference, 1)}", Color.Green,
            TimeSpan.FromMilliseconds(300), PlayerInstance);
    }

    private void GiveEnergyBuff(IPlayer playerInstance, float healAmount)
    {
        var modifiers = playerInstance.GetModifiers();
        modifiers.EnergyRechargeModifier = BuffedEnergyRechargeModifier;
        modifiers.EnergyConsumptionModifier = BuffedEnergyConsumptionModifier;
        modifiers.CurrentEnergy += healAmount * 2;
        playerInstance.SetModifiers(modifiers);

        _energyRechargeBuffTimer.Restart();
    }

    private void OnWeaponAdded(Event<PlayerWeaponAddedArg> @event)
    {
        var args = @event.Args;
        if (args.WeaponItemType != WeaponItemType.InstantPickup)
            return;

        var damage = args.WeaponItem switch
        {
            WeaponItem.MEDKIT => MedkitHealthEffect + PoisonAmount,
            WeaponItem.PILLS => PillsHealthEffect + PoisonAmount,
            _ => 0
        };

        if (damage == 0)
            return;

        var poison = new Poison(Player, damage, _notificationService);
        _buffsService.TryAddBuff(poison, Player);
    }

    private void OnBloodEffectCallback()
    {
        if (!Player.IsInstanceValid())
            return;

        _effectsPlayer.PlayEffect(EffectName.Blood, PlayerInstance!.GetWorldPosition());
    }

    private void OnRestoreEnergyModifier()
    {
        var modifiers = PlayerInstance!.GetModifiers();

        modifiers.EnergyRechargeModifier = EnergyRechargeModifier;
        modifiers.EnergyConsumptionModifier = EnergyConsumptionModifier;
        PlayerInstance.SetModifiers(modifiers);
    }

    private void UpdateDialogue()
    {
        ShowPersistentDialogue("VAMPIRE");
    }
}