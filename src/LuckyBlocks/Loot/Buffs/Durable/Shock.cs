using System;
using System.Diagnostics.CodeAnalysis;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class Shock : DurableBuffBase, IRepressibleByImmunityFlagsBuff
{
    public override string Name => "Shock";
    public override TimeSpan Duration => TimeSpan.FromSeconds(5);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToShock;

    protected override Color BuffColor => ExtendedColors.Electric;

    private static TimeSpan ShockDamagePeriod => TimeSpan.FromMilliseconds(300);

    private const float SHOCK_DAMAGE = 3f;

    private static IObject? _invisibleBlock;

    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    private readonly IGame _game;

    private Vector2 _oldPosition;
    private IPlayer? _fakePlayer;
    private PeriodicTimer? _shockDamageTimer;

    public Shock(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default, bool cloned = false) : base(
        player, args, timeLeft, cloned)
    {
        _notificationService = args.NotificationService;
        _args = args;
        _game = args.Game;
        _invisibleBlock ??= _game.CreateObject("InvisibleBlock", args.Game.GetCameraArea().TopLeft);
    }

    public override IDurableBuff Clone()
    {
        return new Shock(Player, _args, TimeLeft, true) { _oldPosition = _oldPosition };
    }

    protected override void OnRan()
    {
        EnableBuff();

        CreateAndStartShockDamageTimer();

        UpdateDialogue(_fakePlayer, true);

        var playerInstance = Player.Instance!;
        
        ExtendedEvents.HookOnDamage(_fakePlayer, OnDamage, EventHookMode.Default);
        ExtendedEvents.HookOnDestroyed(_fakePlayer, OnFakePlayerRemovedOrParentDead, EventHookMode.Default);
        ExtendedEvents.HookOnDead(playerInstance, OnFakePlayerRemovedOrParentDead, EventHookMode.Default);
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue(_fakePlayer!, true);

        CreateAndStartShockDamageTimer();

        _notificationService.CreateChatNotification($"You're shocked again for {TimeLeft.TotalSeconds}s", BuffColor,
            Player.UserIdentifier);
    }

    protected override void OnFinished()
    {
        CloseDialogue(_fakePlayer);

        ExtendedEvents.Clear();

        DisableBuff();
    }

    [MemberNotNull(nameof(_fakePlayer))]
    private void EnableBuff()
    {
        var playerInstance = Player.Instance!;

        if (!Cloned)
        {
            _oldPosition = playerInstance.GetWorldPosition();
        }

        _fakePlayer = _game.CreatePlayer(_oldPosition);
        _fakePlayer.SetProfile(Player.Profile);
        _fakePlayer.SetBotName($"{Player.Name} (fake)");
        _fakePlayer.Kill();

        playerInstance.SetWorldPosition(_invisibleBlock!.GetWorldPosition() + new Vector2(0, 16));
        playerInstance.SetInputMode(PlayerInputMode.Disabled);
        playerInstance.SetLinearVelocity(Vector2.Zero);
        playerInstance.SetAngularVelocity(0f);
        playerInstance.SetNametagVisible(false);
    }

    private void DisableBuff()
    {
        var position = _fakePlayer?.GetWorldPosition() ?? default;
        if (position == default)
        {
            position = _oldPosition;
        }
        
        var playerInstance = Player.Instance;
        
        if (_fakePlayer?.IsValid() == true && playerInstance?.IsValid() == true && !playerInstance.IsDead)
        {
            _fakePlayer?.RemoveDelayed();
        }
        
        if (playerInstance?.IsDead != false)
            return;
        
        playerInstance.SetWorldPosition(position);
        playerInstance.SetInputMode(PlayerInputMode.Enabled);
        playerInstance.SetNametagVisible(true);
    }

    private void OnDamage(Event<PlayerDamageArgs> @event)
    {
        var args = @event.Args;
        
        var playerInstance = Player.Instance;
        if (!Player.IsValid())
            return;

        var currentHealth = playerInstance!.GetHealth();
        if (args.Damage >= currentHealth)
        {
            Awaiter.Start(playerInstance.Kill, TimeSpan.Zero);
            return;
        }
        
        playerInstance.SetHealth(playerInstance.GetHealth() - args.Damage);
    }
    
    private void CreateAndStartShockDamageTimer()
    {
        _shockDamageTimer?.Stop();

        _shockDamageTimer = new PeriodicTimer(ShockDamagePeriod, TimeBehavior.TimeModifier, OnShockDamageTick, default,
            (int)TimeLeft.Divide(ShockDamagePeriod), ExtendedEvents);
        _shockDamageTimer.Start();
    }

    private void OnShockDamageTick()
    {
        var playerInstance = Player.Instance;
        if (!Player.IsValid() || playerInstance!.IsDead)
            return;

        if (_fakePlayer is null || !_fakePlayer.IsValid())
            return;

        playerInstance.SetHealth(playerInstance.GetHealth() - SHOCK_DAMAGE);

        _notificationService.CreateTextNotification($"-{SHOCK_DAMAGE}", ExtendedColors.Electric,
            TimeSpan.FromMilliseconds(1000), _fakePlayer);
    }
    
    private void OnFakePlayerRemovedOrParentDead(Event @event)
    {
        RemovePlayer();
        ExternalFinish();
    }
    
    private void RemovePlayer()
    {
        var playerInstance = Player.Instance;
        playerInstance?.RemoveDelayed();
    } 
    
    private void UpdateDialogue(IPlayer player, bool ignoreDeath)
    {
        ShowDialogue("SHOCKED", BuffColor, TimeLeft, player, ignoreDeath);
    }
}