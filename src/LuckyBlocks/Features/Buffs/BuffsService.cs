using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
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
    private static TimeSpan ImmunityMessagesCooldown => TimeSpan.FromMilliseconds(500);

    private readonly IImmunityService _immunityService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly Dictionary<IPlayer, float> _lastImmunityMessages = new();

    public BuffsService(IImmunityService immunityService, IEffectsPlayer effectsPlayer, IGame game, ILogger logger)
    {
        _immunityService = immunityService;
        _effectsPlayer = effectsPlayer;
        _game = game;
        _logger = logger;
    }

    public OneOf<Success, PlayerIsDeadResult, ImmunityFlag> TryAddBuff(IBuff buff, Player player)
    {
        var playerInstance = player.Instance;
        if (!player.IsInstanceValid() || playerInstance!.IsDead)
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

        if (buff is IImmunityFlagsIndicatorBuff)
        {
            GiveBuffImmunities(finishableBuff, player);
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
        {
            return new Success();
        }

        if (_lastImmunityMessages.TryGetValue(playerInstance, out var lastMessageTime))
        {
            if (_game.TotalElapsedRealTime - lastMessageTime < ImmunityMessagesCooldown.TotalMilliseconds)
            {
                return incompatibleFlags;
            }
        }

        _lastImmunityMessages[playerInstance] = _game.TotalElapsedRealTime;
        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, playerInstance.GetWorldPosition(), "Immunity");

        return incompatibleFlags;
    }

    private void GiveBuffImmunities(IFinishableBuff buff, Player player)
    {
        var immunities = _immunityService.GiveImmunitiesFromBuff(player, (buff as IImmunityFlagsIndicatorBuff)!);
        foreach (var immunity in immunities)
        {
            buff
                .WhenFinish
                .Invoke(_ => _immunityService.RemoveImmunity(player, immunity));
        }
    }
}