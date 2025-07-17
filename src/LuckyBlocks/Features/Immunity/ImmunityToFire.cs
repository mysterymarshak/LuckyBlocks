using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.PlayerModifiers;
using SFDPlayerModifiers = SFDGameScriptInterface.PlayerModifiers;

namespace LuckyBlocks.Features.Immunity;

internal class ImmunityToFire : IApplicableImmunity, IDelayedRemoveImmunity
{
    public static readonly SFDPlayerModifiers ModifiedModifiers = new()
    {
        CanBurn = 0
    };

    public string Name => "Immunity to fire";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToFire;
    public TimeSpan RemovalDelay => TimeSpan.FromSeconds(2);

    private readonly Player _player;
    private readonly IPlayerModifiersService _playerModifiersService;

    private SFDPlayerModifiers? _playerModifiers;

    public ImmunityToFire(Player player, ImmunityConstructorArgs args)
        => (_player, _playerModifiersService) = (player, args.PlayerModifiersService);

    public void Apply()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        _playerModifiers = playerInstance.GetModifiers();
        _playerModifiersService.AddModifiers(_player, ModifiedModifiers);
    }

    public void Remove()
    {
        _playerModifiersService.RevertModifiers(_player, ModifiedModifiers, _playerModifiers!);
    }
}