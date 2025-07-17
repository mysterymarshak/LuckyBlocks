using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;

namespace LuckyBlocks.Features.WeaponPowerups;

internal interface IPowerupFactory
{
    IWeaponPowerup<Weapon> CreatePowerup(Weapon weapon, Type powerupType);
    T CreatePowerup<T>(Weapon weapon) where T : IWeaponPowerup<Weapon>;
}

internal class PowerupFactory : IPowerupFactory
{
    private readonly PowerupConstructorArgs _args;

    public PowerupFactory(PowerupConstructorArgs args)
        => (_args) = (args);

    public IWeaponPowerup<Weapon> CreatePowerup(Weapon weapon, Type powerupType)
    {
        return (IWeaponPowerup<Weapon>)Activator.CreateInstance(powerupType,
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, [weapon, _args],
            CultureInfo.CurrentCulture);
    }

    public T CreatePowerup<T>(Weapon weapon) where T : IWeaponPowerup<Weapon>
    {
        return (T)Activator.CreateInstance(typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, [weapon, _args],
            CultureInfo.CurrentCulture);
    }
}