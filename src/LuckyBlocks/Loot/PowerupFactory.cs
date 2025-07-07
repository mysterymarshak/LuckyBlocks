using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Loot.WeaponPowerups;

namespace LuckyBlocks.Loot;

internal interface IPowerupFactory
{
    IWeaponPowerup<Weapon> CreatePowerup(Weapon weapon, Type powerupType);
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
            BindingFlags.OptionalParamBinding, null, new object[] { weapon, _args },
            CultureInfo.CurrentCulture);
    }
}