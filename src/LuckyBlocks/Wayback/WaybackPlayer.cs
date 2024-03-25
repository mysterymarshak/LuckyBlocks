using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Wayback;

internal class WaybackPlayer : WaybackObject
{
    private readonly string _name;
    private readonly bool _isDead;
    private readonly PlayerTeam _team;
    private readonly bool _isValidBotEliminateTarget;
    private readonly BotBehavior _botBehavior;
    private readonly BotBehaviorSet _botBehaviorSet;
    private readonly bool _botBehaviorActive;
    private readonly IObject _guardTarget;
    private readonly IObject _forcedBotTarget;
    private readonly IUser? _user;
    private readonly IProfile _profile;
    private readonly bool _isNameTagVisible;
    private readonly bool _isStatusBarVisible;
    private readonly float _energy;
    private readonly PlayerHitEffect _hitEffect;
    private readonly bool _isStrengthBoostActive;
    private readonly float _strengthBoostTime;
    private readonly bool _isBoostHealthActive; // ?
    private readonly float _corspeHealth; // ?
    private readonly bool _isSpeedBoostActive;
    private readonly float _speedBoostTime;
    private readonly WeaponsData _weaponsData;
    private readonly PlayerInputMode _inputMode;
    private readonly PlayerModifiers _playerModifiers; // ?
    private readonly IState _playerState;

    private readonly IIdentityService _identityService;
    private readonly IRespawner _respawner;
    
    private IPlayer _playerInstance;
    
    public WaybackPlayer(IPlayer playerInstance, IIdentityService identityService, IRespawner respawner) : base(playerInstance)
    {
        _playerInstance = playerInstance;
        _name = playerInstance.Name;
        _isDead = playerInstance.IsDead;
        _team = playerInstance.GetTeam();
        _isValidBotEliminateTarget = playerInstance.IsValidBotEliminateTarget;
        _botBehavior = playerInstance.GetBotBehavior();
        _botBehaviorSet = playerInstance.GetBotBehaviorSet();
        _botBehaviorActive = playerInstance.GetBotBehaivorActive();
        _guardTarget = playerInstance.GetGuardTarget();
        _forcedBotTarget = playerInstance.GetForcedBotTarget();
        _user = playerInstance.GetUser();
        _profile = playerInstance.GetProfile();
        _isNameTagVisible = playerInstance.GetNametagVisible();
        _isStatusBarVisible = playerInstance.GetStatusBarsVisible();
        _energy = playerInstance.GetEnergy();
        _hitEffect = playerInstance.GetHitEffect();
        _isStrengthBoostActive = playerInstance.IsStrengthBoostActive;
        _strengthBoostTime = playerInstance.GetStrengthBoostTime();
        _isSpeedBoostActive = playerInstance.IsSpeedBoostActive;
        _speedBoostTime = playerInstance.GetSpeedBoostTime();
        _weaponsData = playerInstance.GetWeaponsData();
        _inputMode = playerInstance.InputMode;
        _playerState = identityService.GetPlayerByInstance(playerInstance).GetState();
        _identityService = identityService;
        _respawner = respawner;
    }

    public override void Restore(IGame game)
    {
        if (_user is null)
            return;
        
        base.Restore(game);
        _playerInstance = (IPlayer)Object;
        
        _playerInstance.SetWeapons(_weaponsData);
        
        var player = _identityService.GetPlayerByInstance(_playerInstance);
        player.RestoreFromState(_playerState);
    }

    protected override IObject Respawn(IGame game, Vector2 position, int direction)
    {
        return _respawner.RespawnPlayer(_user!, _profile, position, direction);
    }

    protected override bool IsValid(IObject @object)
    {
        if (!_playerInstance.IsValid())
            return false;

        return !(_playerInstance.IsDead && !_isDead);
    }
}