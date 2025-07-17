using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using SFDPlayerModifiers = SFDGameScriptInterface.PlayerModifiers;

namespace LuckyBlocks.Features.PlayerModifiers;

internal interface IPlayerModifiersService
{
    void AddModifiers(Player player, SFDPlayerModifiers playerModifiers);

    void RevertModifiers(Player player, SFDPlayerModifiers modifiersToRevert,
        SFDPlayerModifiers backedUpPlayerModifiers);

    bool IsConflictedWith(Player player, SFDPlayerModifiers checkableModifiers);
}

internal class PlayerModifiersService : IPlayerModifiersService
{
    public void AddModifiers(Player player, SFDPlayerModifiers addedModifiers)
    {
        var playerModifiers = player.ModifiedModifiers.Concat(addedModifiers);
        player.ModifiedModifiers = playerModifiers;

        var playerInstance = player.Instance;
        playerInstance?.SetModifiers(playerModifiers);
    }

    public void RevertModifiers(Player player, SFDPlayerModifiers modifiersToRevert,
        SFDPlayerModifiers backedUpPlayerModifiers)
    {
        var playerModifiers = player.ModifiedModifiers;
        var revertedModifiers = modifiersToRevert.Revert(backedUpPlayerModifiers);

        var playerInstance = player.Instance;
        playerInstance?.SetModifiers(revertedModifiers);

        player.ModifiedModifiers = playerModifiers.Except(modifiersToRevert);
    }

    public bool IsConflictedWith(Player player, SFDPlayerModifiers checkableModifiers)
    {
        return player.ModifiedModifiers.IsConflictedWith(checkableModifiers);
    }
}