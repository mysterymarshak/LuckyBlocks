using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedProjectile : IRevertedEntity
{
    public int InstanceId => _projectile.InstanceID;

    private readonly IProjectilesService _projectilesService;
    private readonly ProjectileItem _projectileItem;
    private readonly Vector2 _position;
    private readonly Vector2 _velocity;
    private readonly Vector2 _direction;
    private readonly int _bounceCount;
    private readonly bool _powerupBounceActive;
    private readonly bool _powerupFireActive;
    private readonly float _damageDealtModifier;
    private readonly float _critChanceDealtModifier;
    private readonly List<IProjectilePowerup> _powerups;

    private IProjectile _projectile;

    public RevertedProjectile(IProjectile projectile, IProjectilesService projectilesService)
    {
        _projectile = projectile;
        _projectilesService = projectilesService;
        _projectileItem = projectile.ProjectileItem;
        _position = projectile.Position;
        _velocity = projectile.Velocity;
        _direction = projectile.Direction;
        _bounceCount = projectile.BounceCount;
        _powerupBounceActive = projectile.PowerupBounceActive;
        _powerupFireActive = projectile.PowerupFireActive;
        _damageDealtModifier = projectile.DamageDealtModifier;
        _critChanceDealtModifier = projectile.CritChanceDealtModifier;
        _powerups = projectilesService.GetPowerupsCopy(projectile).ToList();
    }

    public void Restore(IGame game)
    {
        if (_projectile.IsRemoved)
        {
            _projectile = SpawnProjectile(game);
        }
        else
        {
            _projectile.Position = _position;

            if (_projectile.Direction != _direction)
            {
                _projectile.Direction = _direction;
            }
        }

        if (_projectile.Velocity != _velocity)
        {
            _projectile.Velocity = _velocity;
        }

        if (_projectile.BounceCount != _bounceCount)
        {
            _projectile.BounceCount = _bounceCount;
        }

        if (_projectile.DamageDealtModifier != _damageDealtModifier)
        {
            _projectile.DamageDealtModifier = _damageDealtModifier;
        }

        if (_projectile.CritChanceDealtModifier != _critChanceDealtModifier)
        {
            _projectile.CritChanceDealtModifier = _critChanceDealtModifier;
        }

        if (_projectile.PowerupBounceActive != _powerupBounceActive)
        {
            _projectile.PowerupBounceActive = _powerupBounceActive;
        }

        if (_projectile.PowerupFireActive != _powerupFireActive)
        {
            _projectile.PowerupFireActive = _powerupFireActive;
        }

        _projectilesService.ApplyPowerups(_projectile, _powerups);
    }

    private IProjectile SpawnProjectile(IGame game)
    {
        return game.SpawnProjectile(_projectileItem, _position, _direction,
            _powerupFireActive ? ProjectilePowerup.Fire :
            _powerupBounceActive ? ProjectilePowerup.Bouncing : ProjectilePowerup.None);
    }
}