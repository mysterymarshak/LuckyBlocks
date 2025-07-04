﻿using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Loot.Buffs.Instant;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class Vampirism : DurableBuffBase
{
    public static readonly PlayerModifiers ModifiedModifiers = new()
    {
        RunSpeedModifier = 1.2f,
        SprintSpeedModifier = 1.2f
    };

    public override string Name => "Vampirism";
    public override TimeSpan Duration => TimeSpan.FromSeconds(15);

    protected override Color BuffColor => ExtendedColors.ImperialRed;

    private const int POISON_AMOUNT = 25;
    private const int MEDKIT_HEALTH_EFFECT = 50;
    private const int PILLS_HEALTH_EFFECT = 25;

    private readonly IBuffsService _buffsService;
    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly IGame _game;
    private readonly BuffConstructorArgs _args;
    private readonly RandomPeriodicTimer _bloodEffectTimer;

    private PlayerModifiers? _playerModifiers;

    public Vampirism(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
    {
        _buffsService = args.BuffsService;
        _notificationService = args.NotificationService;
        _effectsPlayer = args.EffectsPlayer;
        _playerModifiersService = args.PlayerModifiersService;
        _game = args.Game;
        _args = args;
        _bloodEffectTimer = new(TimeSpan.FromMilliseconds(600), TimeSpan.FromMilliseconds(1000),
            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop, OnBloodEffectCallback, ExtendedEvents);
    }

    public override IDurableBuff Clone()
    {
        return new Vampirism(Player, _args, TimeLeft);
    }
    
    protected override void OnRan()
    {
        var playerInstance = Player.Instance!;
        _playerModifiers = playerInstance.GetModifiers();

        EnableBuff();

        _bloodEffectTimer.Start();
        UpdateDialogue();
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        _notificationService.CreateChatNotification($"You're blood sucker again for {TimeLeft.TotalSeconds}s",
            BuffColor, Player.UserIdentifier);
    }

    protected override void OnFinished()
    {
        DisableBuff();

        ExtendedEvents.Clear();
        _bloodEffectTimer.Stop();
    }

    private void EnableBuff()
    {
        var playerInstance = Player.Instance!;

        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);

        ExtendedEvents.HookOnDamage(OnDamage, EventHookMode.Default);
        ExtendedEvents.HookOnWeaponAdded(playerInstance, OnWeaponAdded, EventHookMode.Default);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
    }

    private void OnDamage(Event<IPlayer, PlayerDamageArgs> @event)
    {
        var (attackedPlayer, args, _) = @event;
        var playerInstance = Player.Instance;
        
        if (attackedPlayer == playerInstance || playerInstance?.IsValidUser() != true || attackedPlayer?.IsValidUser() != true)
            return;
        
        switch (args.DamageType)
        {
            case PlayerDamageEventType.Melee when args.SourceID != playerInstance.UniqueId:
                return;
            case PlayerDamageEventType.Projectile:
            {
                var projectile = _game.GetProjectile(args.SourceID);
                if (projectile.InitialOwnerPlayerID != playerInstance.UniqueId)
                    return;
                
                break;
            }
        }

        OnAttack(args);
    }

    private void OnAttack(PlayerDamageArgs args)
    {
        var playerInstance = Player.Instance!;
        var health = playerInstance.GetHealth();
        var maxHealth = playerInstance.GetMaxHealth();
        var newHealth = Math.Min(maxHealth, health + args.Damage);
        var difference = newHealth - health;

        if (difference <= 0)
            return;

        playerInstance.SetHealth(newHealth);

        _notificationService.CreateTextNotification($"+{Math.Round(difference, 1)}", Color.Green,
            TimeSpan.FromMilliseconds(300), playerInstance);
    }

    private void OnWeaponAdded(Event<PlayerWeaponAddedArg> @event)
    {
        var args = @event.Args;
        if (args.WeaponItemType != WeaponItemType.InstantPickup)
            return;

        var damage = args.WeaponItem switch
        {
            WeaponItem.MEDKIT => MEDKIT_HEALTH_EFFECT + POISON_AMOUNT,
            WeaponItem.PILLS => PILLS_HEALTH_EFFECT + POISON_AMOUNT,
            _ => 0
        };

        if (damage == 0)
            return;

        var poison = new Poison(Player, damage, _notificationService);
        _buffsService.TryAddBuff(poison, Player);
    }

    private void OnBloodEffectCallback()
    {
        if (!Player.IsValid())
            return;

        var playerInstance = Player.Instance!;
        _effectsPlayer.PlayEffect(EffectName.Blood, playerInstance.GetWorldPosition());
    }

    private void UpdateDialogue()
    {
        ShowDialogue("VAMPIRE", BuffColor, TimeLeft);
    }
}