using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Extensions;

internal static class PlayerExtensions
{
    public static bool IsValid(this Player player)
    {
        return player.Instance?.IsValid() ?? false;
    }
    
    public static bool HasBuff(this Player player, Type buffType)
    {
        var buffs = player.Buffs;
        return buffs.Any(buffType.IsInstanceOfType);
    }
    
    public static bool HasAnyOfBuffs(this Player player, IEnumerable<Type> buffTypes,
        IEnumerable<Type>? exclusions = default)
    {
        var buffs = player.Buffs;
        return buffs.Any(x =>
            buffTypes.Any(y => y.IsInstanceOfType(x)) && exclusions?.Any(z => z.IsInstanceOfType(x)) == false);
    }

    public static ImmunityFlag GetImmunityFlags(this Player player)
    {
        var immunities = player.Immunities;
        
        if (!immunities.Any())
            return ImmunityFlag.None;
        
        return immunities
            .Select(x => x.Flag)
            .Aggregate((x, y) => x | y);
    }

    public static bool HasAnyWeapon(this Player player)
    {
        var playerInstance = player.Instance;
        return playerInstance?.HasAnyWeapon() ?? false;
    }
}