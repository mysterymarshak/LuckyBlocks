using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot.Buffs.Wizards;
using LuckyBlocks.Utils;
using LuckyBlocks.Wayback;
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
        IWeaponsPowerupsService weaponsPowerupsService, IPlayerModifiersService playerModifiersService,
        IComparer<ItemAttribute> attributesComparer, IWaybackMachine waybackMachine, IGame game)
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
            return identityService.GetAlivePlayers()
                .Take(2)
                .Count() == 2;
        }

        bool AnyPlayerHaveAnyWeaponAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return identityService.GetAlivePlayers()
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
            var playerInstance = player.AsT0.Instance;
            playerInstance.GetUnsafeWeaponsData(out var weaponsData);

            return weaponsData.HasAnyFirearm();
        }

        bool PlayerHasGotWeaponsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var weaponsToFind = (attribute as PlayerHasGotWeaponsAttribute)!.WeaponItems;
            var playerInstance = player.AsT0.Instance;
            playerInstance.GetUnsafeWeaponsData(out var weaponsData);

            return weaponsData.WeaponsExists(weaponsToFind);
        }

        bool IncompatibleWithPowerupsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var playerInstance = player.AsT0.Instance;
            var typedAttribute = (attribute as IncompatibleWithPowerupsAttribute)!;

            return weaponsPowerupsService.CanAddWeaponPowerup(playerInstance, typedAttribute.SourcePowerup,
                typedAttribute.Types);
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

        bool CantBeAppliedIfAlreadyExistsAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var type = (attribute as CantBeAppliedIfAlreadyExists)!.BuffType;
            return player.AsT0.Buffs.All(x => x.GetType() != type);
        }

        bool WaybackMachineCanBeUsedAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            return waybackMachine.CanBeUsed;
        }

        bool NoOneHaveBuffAttributeCheck(ItemAttribute attribute, OneOf<Player, Unknown> player)
        {
            var type = (attribute as NoOneHaveBuffAttribute)!.BuffType;
            return identityService.GetAlivePlayers().All(x => !x.HasBuff(type) || x == player.AsT0);
        }

        _checks = new Dictionary<Type, Func<ItemAttribute, OneOf<Player, Unknown>, bool>>
        {
            [typeof(UnusedAttribute)] = (_, _) => false,
            [typeof(DisabledAttribute)] = (_, _) => false,
            [typeof(OnlyPlayerAttribute)] = OnlyPlayerAttributeCheck,
            [typeof(PlayerIsNotFullHpAttribute)] = PlayerIsNotFullHpAttributeCheck,
            [typeof(BarrelExistsAttribute)] = BarrelExistsAttributeCheck,
            [typeof(WaybackMachineCanBeUsedAttribute)] = WaybackMachineCanBeUsedAttributeCheck,
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
            [typeof(CantBeAppliedIfAlreadyExists)] = CantBeAppliedIfAlreadyExistsAttributeCheck,
            [typeof(NoOneHaveBuffAttribute)] = NoOneHaveBuffAttributeCheck
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