using System.Collections.Generic;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities.TimeStop;

internal class TimeStoppedPlayer : TimeStoppedDynamicObjectBase
{
    private readonly IPlayer _playerInstance;
    private readonly Player _player;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly List<IObject> _prohibitMoveObjects = [];

    private float _activeThrowableTimer;
    private IEventSubscription? _damageEventSubscription;
    private IEventSubscription? _meleeActionEventSubscription;
    private float _delayedDamage;
    private float _meleeHits;
    private bool _isFall;
    private PlayerModifiers? _playerModifiers;

    public TimeStoppedPlayer(IPlayer player, IGame game, IEffectsPlayer effectsPlayer, IExtendedEvents extendedEvents,
        IPlayerModifiersService playerModifiersService,
        IIdentityService identityService) : base(player, game, effectsPlayer, extendedEvents) =>
        (_playerInstance, _player, _playerModifiersService) = (player, identityService.GetPlayerByInstance(player), playerModifiersService);

    protected override void InitializeInternal()
    {
        _activeThrowableTimer = _playerInstance.GetActiveThrowableTimer();
        _playerInstance.SetInputMode(PlayerInputMode.Disabled);

        ProhibitFromGrabbingAndMove();

        _playerModifiers = _playerInstance.GetModifiers();
        _playerModifiersService.AddModifiers(_player, new PlayerModifiers { MeleeStunImmunity = 1 });

        _damageEventSubscription = ExtendedEvents.HookOnDamage(_playerInstance, OnDamage, EventHookMode.Default);
        _meleeActionEventSubscription = ExtendedEvents.HookOnPlayerMeleeAction(OnMeleeAction, EventHookMode.Default);
    }

    protected override void ResumeTimeInternal()
    {
        if (_isFall)
        {
            _playerInstance.AddCommand(new PlayerCommand(PlayerCommandType.Fall));
        }

        _playerInstance.SetInputMode(PlayerInputMode.Enabled);
        _playerInstance.DealDamage(_delayedDamage);
        
        _playerModifiersService.RevertModifiers(_player, new PlayerModifiers { MeleeStunImmunity = 1 }, _playerModifiers!);
    }

    protected override void OnUpdate()
    {
        if (_playerInstance.GetActiveThrowableWeaponItem() != WeaponItem.NONE)
        {
            _playerInstance.SetActiveThrowableTimer(_activeThrowableTimer);
        }
    }

    protected override void DisposeInternal()
    {
        _damageEventSubscription?.Dispose();
        _meleeActionEventSubscription?.Dispose();
        _prohibitMoveObjects.ForEach(x => x.RemoveDelayed());
    }

    private void OnDamage(Event<PlayerDamageArgs> @event)
    {
        var args = @event.Args;

        _playerInstance.SetHealth(_playerInstance.GetHealth() + args.Damage);

        if (args.DamageType == PlayerDamageEventType.Fire)
            return;

        _delayedDamage += args.Damage * 0.7f;
    }

    // source idea: https://steamcommunity.com/sharedfiles/filedetails/?id=2061774989
    // thank to authors for this logic, i think it's the best compromise, a middle between fun and overpowered strength 
    // also for grabbing and movement prohibition idea
    private void OnMeleeAction(Event<IPlayer, PlayerMeleeHitArg[]> @event)
    {
        var attacker = @event.Arg1;
        var meleeHits = @event.Arg2;

        foreach (var meleeHit in meleeHits)
        {
            if (meleeHit.HitObject != _playerInstance)
                continue;

            _meleeHits++;

            var additionalVelocity = Vector2.Zero;

            if (attacker.IsKicking && (attacker.GetWorldPosition().Y > _playerInstance.GetWorldPosition().Y ||
                                       _meleeHits % 3 == 0))
            {
                additionalVelocity += new Vector2(3f, 2.75f);
            }

            if (attacker.IsJumpKicking)
            {
                additionalVelocity += new Vector2(4f, 3.5f);
            }

            if (attacker.IsJumpAttacking)
            {
                additionalVelocity += new Vector2(1.5f, -2.5f);
            }

            if (attacker.IsMeleeAttacking && _meleeHits % 3 == 0)
            {
                additionalVelocity += new Vector2(3.5f, 4f);
            }

            additionalVelocity *= new Vector2(attacker.FacingDirection, 1);
            LinearVelocity += additionalVelocity;

            if (_meleeHits % 3 == 0)
            {
                _isFall = true;
            }

            if (attacker is { IsMeleeAttacking: true, IsWalking: true } && _playerInstance.CurrentWeaponDrawn != WeaponItemType.NONE)
            {
                _playerInstance.Disarm(_playerInstance.CurrentWeaponDrawn);
            }
        }
    }

    private void ProhibitFromGrabbingAndMove()
    {
        var position = _playerInstance.GetWorldPosition();

        var blockNoCollision = Game.CreateObject("InvisibleBlockNoCollision", position);

        var targetJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", position);
        targetJoint.SetTargetObject(blockNoCollision);

        var pullJoint = (IObjectPullJoint)Game.CreateObject("PullJoint", position);
        pullJoint.SetTargetObjectJoint(targetJoint);
        pullJoint.SetTargetObject(_playerInstance);
        pullJoint.SetForce(50f);

        _prohibitMoveObjects.AddRange([blockNoCollision, targetJoint, pullJoint]);
    }
}