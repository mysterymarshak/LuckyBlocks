using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Reflection;
using Serilog;
using SFDPlayerModifiers = SFDGameScriptInterface.PlayerModifiers;

namespace LuckyBlocks.Features.PlayerModifiers;

internal interface IPlayerModifiersService
{
    SFDPlayerModifiers DecoysModifiers { get; }
    void AddModifiers(Player player, SFDPlayerModifiers playerModifiers);

    void RevertModifiers(Player player, SFDPlayerModifiers modifiersToRevert,
        SFDPlayerModifiers backedUpPlayerModifiers);

    bool IsConflictedWith(Player player, SFDPlayerModifiers checkableModifiers);
}

internal class PlayerModifiersService : IPlayerModifiersService
{
    public SFDPlayerModifiers DecoysModifiers => new()
    {
        MeleeDamageTakenModifier = 2f,
        ExplosionDamageTakenModifier = 2f,
        FireDamageTakenModifier = 2f,
        ProjectileCritChanceTakenModifier = 2f,
        ImpactDamageTakenModifier = 2f,
        ProjectileDamageTakenModifier = 2f,
        ProjectileDamageDealtModifier = 0,
        ProjectileCritChanceDealtModifier = 0,
        MeleeForceModifier = 0,
        MeleeDamageDealtModifier = 0,
        ItemDropMode = 2,
        ThrowForce = 1f
    };

    public void AddModifiers(Player player, SFDPlayerModifiers addedModifiers)
    {
        if (player.IsFake())
        {
            addedModifiers = DecoysModifiers.Concat(addedModifiers);
        }

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