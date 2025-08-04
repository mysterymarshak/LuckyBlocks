using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs.Wizards;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Attributes;

internal interface IAttributesChecker
{
    bool Check(Item item, OneOf<Player, Unknown> player, bool ignorePlayerAttributes = false);
}

internal class AttributesChecker : IAttributesChecker
{
    private readonly IComparer<ItemAttribute> _attributesComparer;
    private readonly Dictionary<Type, Func<ItemAttribute, OneOf<Player, Unknown>, bool>> _checks;

    public AttributesChecker(IIdentityService identityService, ISpawnChanceService spawnChanceService,
        IWeaponPowerupsService weaponPowerupsService, IPlayerModifiersService playerModifiersService,
        IComparer<ItemAttribute> attributesComparer, IMagicService magicService, IGame game)
    {
        _attributesComparer = attributesComparer;

        bool OnlyPlayerAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> value)
        {
            if (value.TryPickT1(out _, out var player))
                return false;

            var playerInstance = player.Instance;
            return playerInstance is { IsRemoved: false, DestructionInitiated: false, IsDead: false };
        }

        bool BarrelExistsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return game.GetObjectsByName("BarrelExplosive").Any();
        }

        bool LuckyBlockDropChanceCanBeIncreasedAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return spawnChanceService.ChanceCanBeIncreased;
        }

        bool DeadPlayerExistsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return identityService.GetDeadUsers().Any();
        }

        bool AlivePlayersMoreThanOneAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return identityService.GetAlivePlayers(false)
                .Take(2)
                .Count() == 2;
        }

        bool AnyPlayerHaveAnyWeaponAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var typedAttribute = (attribute as AnyPlayerHaveAnyWeaponAttribute)!;

            return identityService.GetAlivePlayers(false)
                .Where(x => !typedAttribute.ExceptActivator || x != player.AsT0)
                .Any(x => x.HasAnyWeapon());
        }

        bool PlayerIsNotFullHpAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var playerInstance = player.AsT0.Instance;
            return playerInstance.GetHealth() < playerInstance.GetMaxHealth();
        }

        bool IncompatibleWithBuffsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var incompatibleBuffs = (attribute as IncompatibleWithBuffsAttribute)!.Types;
            return !player.AsT0.HasAnyOfBuffs(incompatibleBuffs);
        }

        bool PlayerHasAnyFirearmAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var weaponsData = player.AsT0.WeaponsData;
            return weaponsData.HasAnyFirearm();
        }

        bool PlayerHasGotWeaponsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var weaponsToFind = (attribute as PlayerHasGotWeaponsAttribute)!.WeaponItems;
            var weaponsData = player.AsT0.WeaponsData;

            return weaponsData.WeaponsExists(weaponsToFind);
        }

        bool IncompatibleWithPowerupsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var playerInstance = player.AsT0.Instance;
            var typedAttribute = (attribute as IncompatibleWithPowerupsAttribute)!;

            return weaponPowerupsService.CanAddWeaponPowerup(playerInstance, typedAttribute.SourcePowerup);
        }

        bool PlayerIsNotOtherWizardAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var sourceWizard = (attribute as PlayerIsNotOtherWizardAttribute)!.SourceWizard;
            return !player.AsT0.HasAnyOfBuffs(new Type[] { typeof(IWizard) }, new Type[] { sourceWizard });
        }

        bool ModifiedModifiersAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var attributeModifiers = (attribute as ModifiedModifiersAttribute)!.PlayerModifiers;
            return !playerModifiersService.IsConflictedWith(player.AsT0, attributeModifiers);
        }

        bool PlayerDoesNotHaveBuffAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var type = (attribute as PlayerDoesNotHaveBuff)!.BuffType;
            return player.AsT0.Buffs.All(x => x.GetType() != type);
        }

        bool NoOneHaveBuffAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var type = (attribute as NoOneHaveBuffAttribute)!.BuffType;
            return identityService.GetAlivePlayers().All(x => !x.HasBuff(type) || x == player.AsT0);
        }

        bool MagicIsAllowedAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return magicService.IsMagicAllowed;
        }

        bool PlayerHasNoImmunitiesAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var immunityFlags = (attribute as PlayerHasNoImmunitiesAttribute)!.ImmunityFlags;
            return player.AsT0.GetImmunityFlags().HasFlag(immunityFlags) == false;
        }

        _checks = new Dictionary<Type, Func<ItemAttribute, OneOf<Player, Unknown>, bool>>
        {
            [typeof(UnusedAttribute)] = (_, _) => false,
            [typeof(DisabledAttribute)] = (_, _) => false,
            [typeof(OnlyPlayerAttribute)] = OnlyPlayerAttributeCheck,
            [typeof(PlayerIsNotFullHpAttribute)] = PlayerIsNotFullHpAttributeCheck,
            [typeof(BarrelExistsAttribute)] = BarrelExistsAttributeCheck,
            [typeof(LuckyBlockDropChanceCanBeIncreasedAttribute)] = LuckyBlockDropChanceCanBeIncreasedAttributeCheck,
            [typeof(DeadPlayerExistsAttribute)] = DeadPlayerExistsAttributeCheck,
            [typeof(AlivePlayersMoreThanOneAttribute)] = AlivePlayersMoreThanOneAttributeCheck,
            [typeof(AnyPlayerHaveAnyWeaponAttribute)] = AnyPlayerHaveAnyWeaponAttributeCheck,
            [typeof(IncompatibleWithBuffsAttribute)] = IncompatibleWithBuffsAttributeCheck,
            [typeof(PlayerHasAnyFirearmAttribute)] = PlayerHasAnyFirearmAttributeCheck,
            [typeof(PlayerHasGotWeaponsAttribute)] = PlayerHasGotWeaponsAttributeCheck,
            [typeof(IncompatibleWithPowerupsAttribute)] = IncompatibleWithPowerupsAttributeCheck,
            [typeof(PlayerIsNotOtherWizardAttribute)] = PlayerIsNotOtherWizardAttributeCheck,
            [typeof(ModifiedModifiersAttribute)] = ModifiedModifiersAttributeCheck,
            [typeof(PlayerDoesNotHaveBuff)] = PlayerDoesNotHaveBuffAttributeCheck,
            [typeof(NoOneHaveBuffAttribute)] = NoOneHaveBuffAttributeCheck,
            [typeof(MagicIsAllowedAttribute)] = MagicIsAllowedAttributeCheck,
            [typeof(PlayerHasNoImmunitiesAttribute)] = PlayerHasNoImmunitiesAttributeCheck
        };
    }

    public bool Check(Item item, OneOf<Player, Unknown> player, bool ignorePlayerAttributes = false)
    {
        return EnumUtils.GetAttributesOfType<ItemAttribute, Item>(item)
            .OrderByDescending(x => x, _attributesComparer)
            .Where(x => !ignorePlayerAttributes || (ignorePlayerAttributes && x.GetType() is { Name: var name } &&
                                                    name.Contains("Player") == name.Contains("Players") &&
                                                    !name.Contains("Incompatible")))
            .All(attribute => !_checks.TryGetValue(attribute.GetType(), out var func) || func(attribute, player));
    }
}

internal class AttributesPriorityComparer : IComparer<ItemAttribute>
{
    public int Compare(ItemAttribute x, ItemAttribute y) => (x, y) switch
    {
        (OnlyPlayerAttribute, not OnlyPlayerAttribute) => 1,
        (not OnlyPlayerAttribute, OnlyPlayerAttribute) => -1,
        _ => 0
    };
}