using System;
using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Dialogues;

internal interface IDialoguesService
{
    int AddDialogue(string message, Color color, TimeSpan displayTime, Player player, bool ignoreDeath = false);
    int AddDialogue(string message, Color color, TimeSpan displayTime, IPlayer player, bool ignoreDeath = false, bool realTime = false);
    void RemoveDialogue(Player player, int id);
    void RemoveDialogue(IPlayer playerInstance, int id);
    IEnumerable<Dialogue> CopyDialogues(IPlayer playerInstance);
    void RestoreDialogues(IPlayer playerInstance, IEnumerable<Dialogue> dialogues);
}

internal class DialoguesService : IDialoguesService
{
    private readonly IGame _game;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger _logger;
    private readonly Dictionary<Player, DialoguesAggregator> _dialoguesByPlayers;
    private readonly Dictionary<IPlayer, DialoguesAggregator> _dialoguesByPlayerInstances;

    public DialoguesService(IGame game, ILifetimeScope lifetimeScope, ILogger logger)
        => (_game, _lifetimeScope, _logger, _dialoguesByPlayers, _dialoguesByPlayerInstances) =
            (game, lifetimeScope, logger, new(), new());

    public int AddDialogue(string message, Color color, TimeSpan displayTime, Player player, bool ignoreDeath = false)
    {
        // IsDead check will return false, if player was dead and respawned in same tick (update didn't call)
        var playerInstance = player.Instance;
        if (playerInstance?.IsValid() != true || (playerInstance.GetHealth() == 0 && !ignoreDeath))
            return default;

        var dialoguesAggregator = GetDialogueAggregator(player);
        return dialoguesAggregator.CreateDialogue(message, color, displayTime);
    }

    public int AddDialogue(string message, Color color, TimeSpan displayTime, IPlayer playerInstance, bool ignoreDeath = false, bool realTime = false)
    {
        // IsDead check will return false, if player was dead and respawned in same tick (update didn't call)
        if (!playerInstance.IsValid() || (playerInstance.GetHealth() == 0 && !ignoreDeath))
            return default;

        var dialoguesAggregator = GetDialogueAggregator(playerInstance);
        return dialoguesAggregator.CreateDialogue(message, color, displayTime, realTime);
    }
    
    public void RemoveDialogue(Player player, int id)
    {
        var dialoguesAggregator = GetDialogueAggregator(player);
        dialoguesAggregator.CloseDialogue(id);
    }
    
    public void RemoveDialogue(IPlayer playerInstance, int id)
    {
        var dialoguesAggregator = GetDialogueAggregator(playerInstance);
        dialoguesAggregator.CloseDialogue(id);
    }

    public IEnumerable<Dialogue> CopyDialogues(IPlayer playerInstance)
    {
        var dialoguesAggregator = GetDialogueAggregator(playerInstance);
        return dialoguesAggregator.CopyDialogues();
    }

    public void RestoreDialogues(IPlayer playerInstance, IEnumerable<Dialogue> dialogues)
    {
        var dialoguesAggregator = GetDialogueAggregator(playerInstance);
        dialoguesAggregator.RestoreDialogues(dialogues);
    }

    private DialoguesAggregator GetDialogueAggregator(Player player)
    {
        if (_dialoguesByPlayers.TryGetValue(player, out var dialoguesAggregator))
            return dialoguesAggregator;

        dialoguesAggregator = new DialoguesAggregator(_game, player, _lifetimeScope.BeginLifetimeScope(), _logger);
        _dialoguesByPlayers.Add(player, dialoguesAggregator);

        return dialoguesAggregator;
    }
    
    private DialoguesAggregator GetDialogueAggregator(IPlayer playerInstance)
    {
        if (_dialoguesByPlayerInstances.TryGetValue(playerInstance, out var dialoguesAggregator))
            return dialoguesAggregator;

        dialoguesAggregator = new DialoguesAggregator(_game, playerInstance, _lifetimeScope.BeginLifetimeScope(), _logger);
        _dialoguesByPlayerInstances.Add(playerInstance, dialoguesAggregator);

        return dialoguesAggregator;
    }
}