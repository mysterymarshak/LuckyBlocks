﻿using System;
using System.Collections.Generic;
using System.Linq;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal abstract class ItemAttribute : Attribute
{
}

internal class AlivePlayersMoreThanOneAttribute : ItemAttribute
{
}

internal class AlwaysAttribute : ItemAttribute
{
}

internal class AnyPlayerHaveAnyWeaponAttribute : ItemAttribute
{
}

internal class BarrelExistsAttribute : ItemAttribute
{
}

internal class DeadPlayerExistsAttribute : ItemAttribute
{
}

internal class DisabledAttribute : ItemAttribute
{
}

internal class IncompatibleWithBuffsAttribute : ItemAttribute
{
    public IEnumerable<Type> Types { get; private set; }

    public IncompatibleWithBuffsAttribute(params Type[]? buffs)
        => Types = buffs ?? Enumerable.Empty<Type>();
}

internal class LuckyBlockDropChanceCanBeIncreasedAttribute : ItemAttribute
{
}

internal class OnlyPlayerAttribute : ItemAttribute
{
}

internal class PlayerIsNotFullHpAttribute : ItemAttribute
{
}

internal class PlayerIsNotOtherWizardAttribute : ItemAttribute
{
    public required Type SourceWizard { get; set; }
}

internal class UnusedAttribute : ItemAttribute
{
}

internal class WaybackMachineCanBeUsedAttribute : ItemAttribute
{
}

internal class PlayerHasAnyFirearmAttribute : ItemAttribute
{
}

internal class PlayerHasGotWeaponsAttribute : ItemAttribute
{
    public IEnumerable<WeaponItem> WeaponItems { get; private set; }

    public PlayerHasGotWeaponsAttribute(params WeaponItem[]? weaponItems)
        => WeaponItems = weaponItems ?? Enumerable.Empty<WeaponItem>();
}

internal class IncompatibleWithPowerupsAttribute : ItemAttribute
{
    public required Type SourcePowerup { get; set; }
    public IEnumerable<Type> Types { get; private set; }

    public IncompatibleWithPowerupsAttribute(params Type[]? powerups)
        => Types = powerups ?? Enumerable.Empty<Type>();
}

internal class ModifiedModifiersAttribute : ItemAttribute
{
    public PlayerModifiers PlayerModifiers { get; private set; }

    public ModifiedModifiersAttribute(Type buff, string playerModifiersFieldName)
        => PlayerModifiers = (PlayerModifiers)buff.GetField(playerModifiersFieldName).GetValue(default);
}

internal class CantBeAppliedIfAlreadyExists : ItemAttribute
{
    public Type BuffType { get; private set; }

    public CantBeAppliedIfAlreadyExists(Type buffType)
        => BuffType = buffType;
}

internal class NoOneHaveBuffAttribute : ItemAttribute
{
    public Type BuffType { get; private set; }

    public NoOneHaveBuffAttribute(Type buffType)
        => BuffType = buffType;
}