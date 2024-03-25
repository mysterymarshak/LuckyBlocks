using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;

namespace LuckyBlocks.Features.Immunity;

internal interface IImmunityService
{
    IEnumerable<IImmunity> GiveImmunities(Player player, ImmunityFlag immunityFlags);
    void RemoveImmunity(Player player, IImmunity immunity);
}

internal class ImmunityService : IImmunityService
{
    private readonly ImmunityConstructorArgs _args;

    public ImmunityService(ImmunityConstructorArgs args)
        => (_args) = (args);
    
    public IEnumerable<IImmunity> GiveImmunities(Player player, ImmunityFlag immunityFlags)
    {
        var playerImmunityFlags = player.GetImmunityFlags();
        var singleImmunityFlags = EnumUtils.GetSingleFlags(immunityFlags);

        foreach (var flag in singleImmunityFlags)
        {
            if ((playerImmunityFlags & flag) != 0)
                continue;
            
            var creationResult = CreateImmunity(player, flag);
            if (!creationResult.TryPickT0(out var immunity, out _))
                continue;

            var alreadyExistingImmunity = player.Immunities.FirstOrDefault(x => x.GetType() == immunity.GetType());
            if (alreadyExistingImmunity is not null)
            {
                player.RemoveImmunity(alreadyExistingImmunity);
            }
            
            player.AddImmunity(immunity);
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
    }

    private OneOf<IImmunity, NotFound> CreateImmunity(Player player, ImmunityFlag flag) => flag switch
    {
        ImmunityFlag.ImmunityToFire => new ImmunityToFire(player, _args),
        ImmunityFlag.ImmunityToFall => new ImmunityToFall(player.Instance!, _args.LifetimeScope.BeginLifetimeScope()),
        ImmunityFlag.ImmunityToDeath => new ImmunityToDeath(player, _args, _args.LifetimeScope.BeginLifetimeScope()),
        ImmunityFlag.ImmunityToPoison => new ImmunityToPoison(),
        ImmunityFlag.ImmunityToFreeze => new ImmunityToFreeze(),
        ImmunityFlag.ImmunityToWind => new ImmunityToWind(),
        ImmunityFlag.ImmunityToShock => new ImmunityToShock(),
        ImmunityFlag.ImmunityToTimeStop => new ImmunityToTimeStop(),
        _ => new NotFound()
    };
}