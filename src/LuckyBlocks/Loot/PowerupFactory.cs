using System;
using System.Globalization;
using System.Reflection;
using LuckyBlocks.Data;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

namespace LuckyBlocks.Loot;

internal interface IPowerupFactory
{
    IWeaponPowerup<Weapon> CreatePowerup(Weapon weapon, Type powerupType);
    IFirearmPowerup CreatePowerup(Firearm firearm, Type powerupType);
    IThrowableItemPowerup<Throwable> CreatePowerup(Throwable throwable, Type powerupType);
}

internal class PowerupFactory : IPowerupFactory
{
    private readonly PowerupConstructorArgs _args;

    public PowerupFactory(PowerupConstructorArgs args)
        => (_args) = (args);

    public IWeaponPowerup<Weapon> CreatePowerup(Weapon weapon, Type powerupType) => weapon switch
    {
        Firearm firearm => CreatePowerup(firearm, powerupType),
        Throwable throwable => CreatePowerup(throwable, powerupType),
        Melee melee => CreatePowerup(melee, powerupType),
        _ => throw new ArgumentOutOfRangeException(nameof(weapon))
    };

    public IFirearmPowerup CreatePowerup(Firearm firearm, Type powerupType)
    {
        return (IFirearmPowerup)Activator.CreateInstance(powerupType,
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, new object[] { firearm, _args },
            CultureInfo.CurrentCulture);
    }

    public IThrowableItemPowerup<Throwable> CreatePowerup(Throwable throwable, Type powerupType)
    {
        return (IThrowableItemPowerup<Throwable>)Activator.CreateInstance(powerupType,
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, new object[] { throwable, _args },
            CultureInfo.CurrentCulture);
    }

    public IWeaponPowerup<Melee> CreatePowerup(Melee melee, Type powerupType)
    {
        return (IWeaponPowerup<Melee>)Activator.CreateInstance(powerupType,
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.OptionalParamBinding, null, new object[] { melee, _args },
            CultureInfo.CurrentCulture);
    }
}