using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Loot.Buffs.Instant;

internal class Disarm : IInstantBuff, IRepressibleByImmunityFlagsBuff
{
    public string Name => "Disarm";
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToWind;

    private readonly Player _player;
    
    public Disarm(Player player)
        => (_player) = (player);
    
    public void Run()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);
        
        var weaponItemType = playerInstance.CurrentWeaponDrawn;
        playerInstance.Disarm(weaponItemType);
    }
}