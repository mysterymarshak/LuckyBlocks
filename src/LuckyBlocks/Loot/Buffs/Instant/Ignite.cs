﻿using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Loot.Buffs.Instant;

internal class Ignite : IInstantBuff, IRepressibleByImmunityFlagsBuff
{
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFire;
    public string Name => "Ignite";

    private readonly Player _player;

    public Ignite(Player player)
        => (_player) = (player);

    public void Run()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);
        
        playerInstance.SetMaxFire();
    }
}