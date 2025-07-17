using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using Serilog;

namespace LuckyBlocks.Features.Immunity;

internal interface IImmunityService
{
    IEnumerable<IImmunity> GiveImmunitiesFromBuff(Player player, IImmunityFlagsIndicatorBuff buff);
    void RemoveImmunity(Player player, IImmunity immunity);
}

internal class ImmunityService : IImmunityService
{
    private readonly ILogger _logger;
    private readonly ImmunityConstructorArgs _args;

    public ImmunityService(ImmunityConstructorArgs args)
    {
        _logger = args.Logger;
        _args = args;
    }

    public IEnumerable<IImmunity> GiveImmunitiesFromBuff(Player player, IImmunityFlagsIndicatorBuff buff)
    {
        var playerImmunityFlags = player.GetImmunityFlags();
        var singleImmunityFlags = EnumUtils.GetSingleFlags(buff.ImmunityFlags);

        foreach (var flag in singleImmunityFlags)
        {
            if ((playerImmunityFlags & flag) != 0)
                continue;

            var creationResult = CreateImmunity(player, flag, buff);
            if (!creationResult.TryPickT0(out var immunity, out _))
                continue;

            var alreadyExistingImmunity = player.Immunities.FirstOrDefault(x => x.GetType() == immunity.GetType());
            if (alreadyExistingImmunity is not null)
            {
                player.RemoveImmunity(alreadyExistingImmunity);
            }

            player.AddImmunity(immunity);
            _logger.Debug("Added immunity '{Immunity}' to player '{Player}'", immunity, player.Name);

            yield return immunity;
        }
    }

    public void RemoveImmunity(Player player, IImmunity immunity)
    {
        if (immunity is IDelayedRemoveImmunity delayedRemoveImmunity)
        {
            Awaiter.Start(() => RemoveImmunityInternal(player, immunity), delayedRemoveImmunity.RemovalDelay);
            return;
        }

        RemoveImmunityInternal(player, immunity);
    }

    private void RemoveImmunityInternal(Player player, IImmunity immunity)
    {
        if (immunity is IApplicableImmunity applicableImmunity)
        {
            applicableImmunity.Remove();
        }

        player.RemoveImmunity(immunity);
        _logger.Debug("Removed immunity '{Immunity}' from '{Player}'", immunity, player.Name);
    }

    private OneOf<IImmunity, NotFound> CreateImmunity(Player player, ImmunityFlag flag, IBuff buff) => flag switch
    {
        ImmunityFlag.ImmunityToFire => new ImmunityToFire(player, _args),
        ImmunityFlag.ImmunityToFall => new ImmunityToFall(player.Instance!, _args.LifetimeScope.BeginLifetimeScope()),
        ImmunityFlag.ImmunityToDeath => new ImmunityToDeath(player, _args, _args.LifetimeScope.BeginLifetimeScope()),
        ImmunityFlag.ImmunityToPoison => new ImmunityToPoison(),
        ImmunityFlag.ImmunityToFreeze => new ImmunityToFreeze(),
        ImmunityFlag.ImmunityToWind => new ImmunityToWind(),
        ImmunityFlag.ImmunityToShock when buff is IDelayedImmunityRemovalBuff { ImmunityRemovalDelay: var removalDelay }
            => new ImmunityToShock(removalDelay),
        ImmunityFlag.ImmunityToShock => new ImmunityToShock(),
        ImmunityFlag.ImmunityToTimeStop => new ImmunityToTimeStop(),
        _ => new NotFound()
    };
}