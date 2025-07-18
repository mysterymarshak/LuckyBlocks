using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs.Durable;
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
    OneOf<Success, PlayerIsDeadResult, ImmunityFlag>
        TryAddBuff(IBuff buff, Player player, bool showImmunityHint = true);

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

    public OneOf<Success, PlayerIsDeadResult, ImmunityFlag> TryAddBuff(IBuff buff, Player player,
        bool showImmunityHint = true)
    {
        var playerInstance = player.Instance;
        if (!player.IsInstanceValid() || playerInstance!.IsDead)
            return new PlayerIsDeadResult();

        if (buff is IRepressibleByImmunityFlagsBuff repressibleByImmunityFlagsBuff)
        {
            var result = TryAddRepressibleByImmunityFlagsBuff(player, repressibleByImmunityFlagsBuff, showImmunityHint);
            if (result.TryPickT1(out var flags, out _))
            {
                return OneOf<Success, PlayerIsDeadResult, ImmunityFlag>.FromT2(flags);
            }
        }

        if (buff is IFinishableBuff finishableBuff)
        {
            finishableBuff
                .WhenFinish
                .Invoke(x => RemoveBuff(x, player));

            if (buff is IImmunityFlagsIndicatorBuff)
            {
                RemoveRepressibleByImmunitiesBuffs(finishableBuff, player, showImmunityHint);
                GiveBuffImmunities(finishableBuff, player);
            }
        }

        player.AddBuff(buff);
        _logger.Debug("Buff {BuffName} added to {PlayerName}", buff.Name, playerInstance.Name);

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
        IRepressibleByImmunityFlagsBuff buff, bool showImmunityHint)
    {
        var playerInstance = player.Instance!;
        var immunityFlags = player.GetImmunityFlags();
        var incompatibleFlags = immunityFlags & buff.ImmunityFlags;
        var canBeApplied = incompatibleFlags == 0;

        if (canBeApplied)
        {
            return new Success();
        }

        if (showImmunityHint)
        {
            ShowImmunityHint(playerInstance, incompatibleFlags.ToStringFast(true));
        }

        return incompatibleFlags;
    }

    private void ShowImmunityHint(IPlayer playerInstance, string? message = null)
    {
        if (_lastImmunityMessages.TryGetValue(playerInstance, out var lastMessageTime))
        {
            if (_game.TotalElapsedRealTime - lastMessageTime < ImmunityMessagesCooldown.TotalMilliseconds)
            {
                return;
            }
        }

        _lastImmunityMessages[playerInstance] = _game.TotalElapsedRealTime;
        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, playerInstance.GetWorldPosition(), message ?? "Immunity");
    }

    private void GiveBuffImmunities(IFinishableBuff buff, Player player)
    {
        var immunities = _immunityService.GiveImmunitiesFromBuff(player, (IImmunityFlagsIndicatorBuff)buff);
        foreach (var immunity in immunities)
        {
            buff
                .WhenFinish
                .Invoke(_ => _immunityService.RemoveImmunity(player, immunity));
        }
    }

    private void RemoveRepressibleByImmunitiesBuffs(IFinishableBuff finishableBuff, Player player,
        bool showImmunityHint)
    {
        var immunityFlags = ((IImmunityFlagsIndicatorBuff)finishableBuff).ImmunityFlags;
        var repressibleByImmunitiesBuffs = player.Buffs
            .OfType<IDurableRepressibleByImmunityFlagsBuff>()
            .ToList();

        foreach (var buff in repressibleByImmunitiesBuffs)
        {
            var repressibleBuffFlags = buff.ImmunityFlags;
            var incompatibleFlags = immunityFlags & repressibleBuffFlags;
            var shouldRepressed = incompatibleFlags != 0;
            if (!shouldRepressed)
                continue;

            buff.Repress(finishableBuff);

            if (showImmunityHint)
            {
                ShowImmunityHint(player.Instance!, repressibleBuffFlags.ToStringFast(true));
            }
        }
    }
}