using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal interface IProjectilePowerupsFactory
{
    IProjectilePowerup CreatePowerup<T>(IProjectile projectile, IExtendedEvents extendedEvents,
        PowerupConstructorArgs args) where T : IProjectilePowerup;
}

internal class ProjectilePowerupsFactory : IProjectilePowerupsFactory
{
    public IProjectilePowerup CreatePowerup<T>(IProjectile projectile, IExtendedEvents extendedEvents,
        PowerupConstructorArgs args) where T : IProjectilePowerup
    {
        return (IProjectilePowerup)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, [projectile, extendedEvents, args], CultureInfo.CurrentCulture);
    }
}