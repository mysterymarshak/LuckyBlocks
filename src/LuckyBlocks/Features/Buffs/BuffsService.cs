using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Loot.Buffs;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs;

internal interface IBuffsService
{
    OneOf<Success, PlayerIsDeadResult, ImmunityFlag> TryAddBuff(IBuff buff, Player player);
    void RemoveAllBuffs(Player player);
}

internal class BuffsService : IBuffsService
{
    private readonly IImmunityService _immunityService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ILogger _logger;

    public BuffsService(IImmunityService immunityService, IEffectsPlayer effectsPlayer, ILogger logger) =>
        (_immunityService, _effectsPlayer, _logger) = (immunityService, effectsPlayer, logger);

    public OneOf<Success, PlayerIsDeadResult, ImmunityFlag> TryAddBuff(IBuff buff, Player player)
    {
        var playerInstance = player.Instance;
        if (!player.IsValid() || playerInstance!.IsDead)
            return new PlayerIsDeadResult();

        if (buff is IRepressibleByImmunityFlagsBuff repressibleByImmunityFlagsBuff)
        {
            var result = TryAddRepressibleByImmunityFlagsBuff(player, repressibleByImmunityFlagsBuff);
            if (result.TryPickT1(out var flags, out _))
                return OneOf<Success, PlayerIsDeadResult, ImmunityFlag>.FromT2(flags);
        }

        player.AddBuff(buff);

        _logger.Debug("Buff {BuffName} added to {PlayerName}", buff.Name, playerInstance.Name);

        if (buff is not IFinishableBuff finishableBuff)
            return new Success();

        finishableBuff
            .WhenFinish
            .Invoke(x => RemoveBuff(x, player));

        if (buff is IImmunityFlagsIndicatorBuff immunityFlagsBuff)
        {
            GiveImmunityBuffs(finishableBuff, immunityFlagsBuff, player);
        }

        return new Success();
    }

    public void RemoveAllBuffs(Player player)
    {
        player.RemoveAllBuffs();
    }

    private void RemoveBuff(IFinishableBuff buff, Player player)
    {
        player.RemoveBuff(buff);

        _logger.Debug("Buff {BuffName} removed from {PlayerName}", buff.Name, player.Name);
    }

    private OneOf<Success, ImmunityFlag> TryAddRepressibleByImmunityFlagsBuff(Player player,
        IRepressibleByImmunityFlagsBuff buff)
    {
        var playerInstance = player.Instance!;
        var immunityFlags = player.GetImmunityFlags();
        var incompatibleFlags = immunityFlags & buff.ImmunityFlags;
        var canBeApplied = incompatibleFlags == 0;

        if (canBeApplied)
            return new Success();

        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, playerInstance.GetWorldPosition(), "Immunity");
        return incompatibleFlags;
    }

    private void GiveImmunityBuffs(IFinishableBuff parentBuff, IImmunityFlagsIndicatorBuff buff, Player player)
    {
        var immunities = _immunityService.GiveImmunities(player, buff.ImmunityFlags);
        foreach (var immunity in immunities)
        {
            parentBuff
                .WhenFinish
                .Invoke(_ => _immunityService.RemoveImmunity(player, immunity));
        }
    }
}