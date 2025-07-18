using System;
using System.Diagnostics.CodeAnalysis;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.Profiles;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class Shock : DurableRepressibleByImmunityFlagsBuffBase, IDelayedImmunityRemovalBuff
{
    public override string Name => "Shock";
    public override TimeSpan Duration => TimeSpan.FromSeconds(5);

    public override ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToShock;
    public TimeSpan ImmunityRemovalDelay => TimeSpan.FromMilliseconds(700);
    public override Color BuffColor => ExtendedColors.Electric;

    private static TimeSpan ShockDamagePeriod => TimeSpan.FromMilliseconds(300);
    private const float ShockDamage = 3f;
    private static IObject? _invisibleBlock;

    private readonly IProfilesService _profileService;
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    private readonly IGame _game;

    private Vector2 _oldPosition;
    private IPlayer? _fakePlayer;
    private PeriodicTimer? _shockDamageTimer;

    public Shock(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
    {
        _profileService = args.ProfilesService;
        _notificationService = args.NotificationService;
        _args = args;
        _game = args.Game;
        _invisibleBlock ??= _game.CreateObject("InvisibleBlock", args.Game.GetCameraArea().TopLeft);
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new Shock(Player, _args, TimeLeft) { _oldPosition = _oldPosition };
    }

    protected override void OnRunInternal()
    {
        EnableBuff();
        CreateAndStartShockDamageTimer();
        UpdateDialogue(_fakePlayer, true);

        ExtendedEvents.HookOnDamage(_fakePlayer, OnDamage, EventHookMode.Default);
        ExtendedEvents.HookOnDestroyed(_fakePlayer, OnFakePlayerRemovedOrParentDead, EventHookMode.Default);
        ExtendedEvents.HookOnDead(PlayerInstance!, OnFakePlayerRemovedOrParentDead, EventHookMode.Default);
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue(_fakePlayer!, true);
        CreateAndStartShockDamageTimer();
        ShowChatMessage($"You're shocked again for {TimeLeft.TotalSeconds}s");
    }

    protected override void OnFinishInternal()
    {
        CloseDialogue(_fakePlayer);
        DisableBuff();
    }

    [MemberNotNull(nameof(_fakePlayer))]
    private void EnableBuff()
    {
        if (!IsCloned)
        {
            _oldPosition = PlayerInstance!.GetWorldPosition();
        }

        var profile = _profileService.GetPlayerProfile(Player);
        _fakePlayer = _game.CreatePlayer(_oldPosition);
        _fakePlayer.SetProfile(profile);
        _fakePlayer.SetBotName($"{Player.Name} (fake)");
        _fakePlayer.SetCameraSecondaryFocusMode(CameraFocusMode.Focus);
        _fakePlayer.Kill();

        Player.ProfileChanged += OnProfileChanged;
        PlayerInstance!.SetWorldPosition(_invisibleBlock!.GetWorldPosition() + new Vector2(0, 16));
        PlayerInstance.SetInputMode(PlayerInputMode.Disabled);
        PlayerInstance.SetLinearVelocity(Vector2.Zero);
        PlayerInstance.SetAngularVelocity(0f);
        PlayerInstance.SetNametagVisible(false);
        PlayerInstance.SetCameraSecondaryFocusMode(CameraFocusMode.Ignore);
    }

    private void DisableBuff()
    {
        var position = _fakePlayer?.GetWorldPosition() ?? default;
        if (position == default)
        {
            position = _oldPosition;
        }

        if (_fakePlayer?.IsValid() == true && Player.IsInstanceValid() && PlayerInstance!.IsDead == false)
        {
            _fakePlayer?.RemoveDelayed();
        }

        Player.ProfileChanged -= OnProfileChanged;

        if (PlayerInstance?.IsDead != false)
            return;

        PlayerInstance.SetWorldPosition(position + new Vector2(0, 5));
        PlayerInstance.SetInputMode(PlayerInputMode.Enabled);
        PlayerInstance.SetNametagVisible(true);
        PlayerInstance.SetCameraSecondaryFocusMode(CameraFocusMode.Focus);
    }

    private void OnProfileChanged(IProfile profile)
    {
        if (_fakePlayer?.IsValid() == true)
        {
            _fakePlayer.SetProfile(profile);
        }
    }

    private void OnDamage(Event<PlayerDamageArgs> @event)
    {
        var args = @event.Args;

        if (!Player.IsInstanceValid())
            return;

        var currentHealth = PlayerInstance!.GetHealth();
        if (args.Damage >= currentHealth)
        {
            Awaiter.Start(PlayerInstance.Kill, TimeSpan.Zero);
            return;
        }

        PlayerInstance.SetHealth(PlayerInstance.GetHealth() - args.Damage);
    }

    private void CreateAndStartShockDamageTimer()
    {
        _shockDamageTimer?.Stop();

        _shockDamageTimer = new PeriodicTimer(ShockDamagePeriod, TimeBehavior.TimeModifier, OnShockDamageTick, null,
            (int)TimeLeft.Divide(ShockDamagePeriod), ExtendedEvents);
        _shockDamageTimer.Start();
    }

    private void OnShockDamageTick()
    {
        if (!Player.IsInstanceValid() || PlayerInstance!.IsDead)
            return;

        if (_fakePlayer?.IsValid() != true)
            return;

        PlayerInstance.SetHealth(PlayerInstance.GetHealth() - ShockDamage);

        _notificationService.CreateTextNotification($"-{ShockDamage}", ExtendedColors.Electric,
            TimeSpan.FromMilliseconds(1000), _fakePlayer);
    }

    private void OnFakePlayerRemovedOrParentDead(Event @event)
    {
        RemovePlayer();
        ExternalFinish();
    }

    private void RemovePlayer()
    {
        PlayerInstance?.RemoveDelayed();
        _fakePlayer?.SetCameraSecondaryFocusMode(CameraFocusMode.Ignore);
    }

    private void UpdateDialogue(IPlayer player, bool ignoreDeath)
    {
        ShowDialogue("SHOCKED", TimeLeft, BuffColor, player, ignoreDeath);
    }
}