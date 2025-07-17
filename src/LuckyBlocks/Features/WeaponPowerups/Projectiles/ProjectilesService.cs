using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal interface IProjectilesService
{
    void AddPowerup<T>(IProjectile projectile, PowerupConstructorArgs args) where T : IProjectilePowerup;
    List<IProjectilePowerup> GetPowerupsCopy(IProjectile projectile);
    void ApplyPowerups(IProjectile projectile, IEnumerable<IProjectilePowerup> powerups);
}

internal class ProjectilesService : IProjectilesService
{
    private readonly IProjectilePowerupsFactory _projectilePowerupsFactory;
    private readonly Dictionary<IProjectile, List<IProjectilePowerup>> _projectiles = new();
    private readonly IExtendedEvents _extendedEvents;

    public ProjectilesService(IProjectilePowerupsFactory projectilePowerupsFactory, ILifetimeScope lifetimeScope)
    {
        _projectilePowerupsFactory = projectilePowerupsFactory;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void AddPowerup<T>(IProjectile projectile, PowerupConstructorArgs args) where T : IProjectilePowerup
    {
        var powerup = _projectilePowerupsFactory.CreatePowerup<T>(projectile, _extendedEvents, args);
        AddPowerup(projectile, powerup);
    }

    public List<IProjectilePowerup> GetPowerupsCopy(IProjectile projectile)
    {
        if (!_projectiles.TryGetValue(projectile, out var powerups))
        {
            return [];
        }

        return powerups
            .Select(x => x.Clone())
            .ToList();
    }

    public void ApplyPowerups(IProjectile projectile, IEnumerable<IProjectilePowerup> powerups)
    {
        foreach (var powerup in powerups)
        {
            AddPowerup(projectile, powerup);
        }
    }

    private void AddPowerup(IProjectile projectile, IProjectilePowerup powerup)
    {
        if (!_projectiles.TryGetValue(projectile, out var powerups))
        {
            powerups = [];
            _projectiles.Add(projectile, powerups);
        }

        powerup.ProjectileRemove += OnProjectileRemoved;
        powerups.Add(powerup);

        if (powerup.IsCloned)
        {
            powerup.MoveTo(projectile);
        }
        else
        {
            powerup.Run();
        }
    }

    private void OnProjectileRemoved(IProjectilePowerup powerup, ProjectileHitArgs hitArgs)
    {
        powerup.ProjectileRemove -= OnProjectileRemoved;

        var projectile = powerup.Projectile;
        var powerups = _projectiles[projectile];
        powerups.Remove(powerup);

        if (powerups.Count == 0)
        {
            _projectiles.Remove(projectile);
        }
    }
}