using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Wayback;

internal class LightWaybackPlayer : LightWaybackObject
{
    private readonly string _name;
    private readonly bool _isDead;
    private readonly IUser? _user;
    private readonly IProfile _profile;
    private readonly float _energy;
    private readonly bool _isStrengthBoostActive;
    private readonly float _strengthBoostTime;
    private readonly bool _isBoostHealthActive; // ?
    private readonly float _corspeHealth; // ?
    private readonly bool _isSpeedBoostActive;
    private readonly float _speedBoostTime;
    private readonly WeaponsData _weaponsData;
    private readonly IState _playerState;

    private readonly IIdentityService _identityService;
    private readonly IRespawner _respawner;
    
    private IPlayer _playerInstance;
    
    public LightWaybackPlayer(IPlayer playerInstance, IIdentityService identityService, IRespawner respawner) : base(playerInstance)
    {
        _playerInstance = playerInstance;
        _name = playerInstance.Name;
        _isDead = playerInstance.IsDead;
        _user = playerInstance.GetUser();
        _profile = playerInstance.GetProfile();
        _energy = playerInstance.GetEnergy();
        _isStrengthBoostActive = playerInstance.IsStrengthBoostActive;
        _strengthBoostTime = playerInstance.GetStrengthBoostTime();
        _isSpeedBoostActive = playerInstance.IsSpeedBoostActive;
        _speedBoostTime = playerInstance.GetSpeedBoostTime();
        _weaponsData = playerInstance.GetWeaponsData();
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