using System;
using System.Collections.Generic;
using System.Linq;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal abstract class ItemAttribute : Attribute;

internal class AlivePlayersMoreThanOneAttribute : ItemAttribute;

internal class AlwaysAttribute : ItemAttribute;

internal class AnyPlayerHaveAnyWeaponAttribute : ItemAttribute
{
    public bool ExceptActivator { get; private set; }

    public AnyPlayerHaveAnyWeaponAttribute(bool exceptActivator = false)
    {
        ExceptActivator = exceptActivator;
    }
}

internal class BarrelExistsAttribute : ItemAttribute;

internal class DeadPlayerExistsAttribute : ItemAttribute;

internal class DisabledAttribute : ItemAttribute;

internal class IncompatibleWithBuffsAttribute : ItemAttribute
{
    public IEnumerable<Type> Types { get; private set; }

    public IncompatibleWithBuffsAttribute(params Type[]? buffs)
        => Types = buffs ?? Enumerable.Empty<Type>();
}

internal class LuckyBlockDropChanceCanBeIncreasedAttribute : ItemAttribute;

internal class OnlyPlayerAttribute : ItemAttribute;

internal class PlayerIsNotFullHpAttribute : ItemAttribute;

internal class PlayerIsNotOtherWizardAttribute : ItemAttribute
{
    public required Type SourceWizard { get; set; }
}

internal class UnusedAttribute : ItemAttribute;

internal class PlayerHasAnyFirearmAttribute : ItemAttribute;

internal class PlayerHasGotWeaponsAttribute : ItemAttribute
{
    public IEnumerable<WeaponItem> WeaponItems { get; private set; }

    public PlayerHasGotWeaponsAttribute(params WeaponItem[]? weaponItems)
        => WeaponItems = weaponItems ?? Enumerable.Empty<WeaponItem>();
}

internal class IncompatibleWithSomePowerupsAttribute : ItemAttribute
{
    public required Type SourcePowerup { get; set; }
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

internal class WeightAttribute : ItemAttribute
{
    public float Weight { get; private set; }

    public WeightAttribute(float weight)
        => Weight = weight;
}

internal class MagicIsAllowedAttribute : ItemAttribute;