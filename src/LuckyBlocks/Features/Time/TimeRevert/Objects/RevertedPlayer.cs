using System.Collections.Generic;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Wizards;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedPlayer : RevertedDynamicObject
{
    private IPlayer? Instance => _player.Instance;

    private readonly Player _player;
    private readonly IRespawner _respawner;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IBuffsService _buffsService;

    private readonly bool _isDead;
    private readonly bool _isStrengthBoostActive;
    private readonly float _strengthBoostTime;
    private readonly float _speedBoostTime;
    private readonly float _corpseHealth;
    private readonly WeaponsData _weaponsDataCopy;
    private readonly List<ICloneableBuff<IBuff>> _buffs;

    public RevertedPlayer(Player player, IRespawner respawner, IWeaponPowerupsService weaponPowerupsService,
        IBuffsService buffsService) : base(player.Instance!)
    {
        _player = player;
        _respawner = respawner;
        _weaponPowerupsService = weaponPowerupsService;
        _buffsService = buffsService;
        _isDead = Instance!.IsDead;
        _isStrengthBoostActive = Instance.IsStrengthBoostActive;
        _strengthBoostTime = Instance.GetStrengthBoostTime();
        _speedBoostTime = Instance.GetSpeedBoostTime();
        _corpseHealth = Instance.GetCorpseHealth();
        _weaponsDataCopy = weaponPowerupsService.CreateWeaponsDataCopy(player);
        _buffs = buffsService.CloneBuffs(player);
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        _weaponPowerupsService.RestoreWeaponsDataFromCopy(_player, _weaponsDataCopy);

        _buffsService.ForceFinishAllBuffs(_player);
        foreach (var buff in _buffs)
        {
            if (buff is not TimeRevertWizard)
            {
                _buffsService.TryAddBuff(buff, _player);
            }
        }
        
        Instance.SetStrengthBoostTime(_strengthBoostTime);
        Instance.SetSpeedBoostTime(_speedBoostTime);
        
        if (_isStrengthBoostActive && _corpseHealth != Instance.GetCorpseHealth())
        {
            Instance.SetCorpseHealth(_corpseHealth);
        }
    }

    protected override IObject? Respawn(IGame game)
    {
        if (!_player.IsValid() || _player.IsFake())
            return null;

        var playerInstance = _respawner.RespawnPlayer(_player.User, _player.Profile, WorldPosition, Direction);
        playerInstance.SetLinearVelocity(LinearVelocity);
        playerInstance.SetAngularVelocity(AngularVelocity);
        playerInstance.SetAngle(Angle);

        return playerInstance;
    }

    protected override bool IsValid(IObject @object)
    {
        return @object.IsValid() && _player.IsInstanceValid() && _isDead == Instance!.IsDead;
    }
}