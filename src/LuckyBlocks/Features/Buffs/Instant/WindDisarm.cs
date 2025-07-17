using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Features.Buffs.Instant;

internal class WindDisarm : IInstantBuff, IRepressibleByImmunityFlagsBuff
{
    public string Name => "Disarm";
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToWind;

    private readonly Player _player;

    public WindDisarm(Player player)
        => (_player) = (player);

    public void Run()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        var weaponItemType = playerInstance.CurrentWeaponDrawn;
        playerInstance.Disarm(weaponItemType);
    }
}