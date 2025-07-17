using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Dialogues;

internal record Dialogue(string Text, Color Color, TimeSpan DisplayTime);

internal class DialoguesAggregator
{
    private IPlayer? PlayerInstance => _playerInstance ?? _player!.Instance;
    private string PlayerName => _playerName ??= _playerInstance?.Name ?? _player!.Name;

    private readonly IGame _game;
    private readonly Player? _player;
    private readonly IPlayer? _playerInstance;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger _logger;
    private readonly Dictionary<int, Dialogue> _dialogues;
    private readonly Dictionary<int, Timer> _timers;

    private string? _playerName;
    private IDialogue? _dialogueObject;
    private int _lastDialogueId;

    public DialoguesAggregator(IGame game, Player player, ILifetimeScope lifetimeScope, ILogger logger)
        => (_game, _player, _lifetimeScope, _logger, _dialogues, _timers) =
            (game, player, lifetimeScope, logger, new(), new());

    public DialoguesAggregator(IGame game, IPlayer playerInstance, ILifetimeScope lifetimeScope, ILogger logger)
        => (_game, _playerInstance, _lifetimeScope, _logger, _dialogues, _timers) =
            (game, playerInstance, lifetimeScope, logger, new(), new());

    public int CreateDialogue(string text, Color color, TimeSpan displayTime, bool realTime = false)
    {
        var (id, dialogue) = CreateAndAddDialogueWithoutUpdate(text, color, displayTime);

        CreateAndStartTimer(id, dialogue, realTime);
        Update();

        return id;
    }

    public void CloseDialogue(int id)
    {
        if (!_dialogues.TryGetValue(id, out var dialogue))
            return;

        var timer = _timers[id];
        timer.Stop();

        _timers.Remove(id);
        _dialogues.Remove(id);

        _logger.Debug("Dialogue {DialogueText} for player {PlayerName} removed", dialogue.Text, PlayerName);

        Update();
    }

    public IEnumerable<Dialogue> CopyDialogues()
    {
        return _dialogues.Select(dialogue =>
            new Dialogue(dialogue.Value.Text, dialogue.Value.Color, _timers[dialogue.Key].TimeLeft));

        // 'with' keyword usage throws security exception
    }

    public void RestoreDialogues(IEnumerable<Dialogue> dialogues)
    {
        RemoveAllDialogues();

        foreach (var dialogue in dialogues)
        {
            _lastDialogueId++;

            AddDialogueWithoutUpdate(_lastDialogueId, dialogue);
            CreateAndStartTimer(_lastDialogueId, dialogue);
        }

        Update();
    }

    public void RemoveAllDialogues()
    {
        if (!_dialogues.Any())
            return;

        _dialogues.Keys
            .ToList()
            .ForEach(CloseDialogue);

        Update();
    }

    private (int, Dialogue) CreateAndAddDialogueWithoutUpdate(string text, Color color, TimeSpan displayTime)
    {
        _lastDialogueId++;

        var dialogue = new Dialogue(text, color, displayTime);
        return AddDialogueWithoutUpdate(_lastDialogueId, dialogue);
    }

    private (int, Dialogue) AddDialogueWithoutUpdate(int id, Dialogue dialogue)
    {
        _dialogues.Add(id, dialogue);

        _logger.Debug("Dialogue {DialogueText} for player {PlayerName} created", dialogue.Text, PlayerName);

        return (id, dialogue);
    }

    private void CreateAndStartTimer(int dialogueId, Dialogue dialogue, bool realTime = false)
    {
        var lifetimeScope = _lifetimeScope.BeginLifetimeScope();
        var timer = new Timer(dialogue.DisplayTime, realTime ? TimeBehavior.RealTime : TimeBehavior.TimeModifier, () =>
        {
            CloseDialogue(dialogueId);
            lifetimeScope.Dispose();
        }, lifetimeScope.Resolve<IExtendedEvents>());

        _timers.Add(dialogueId, timer);
        timer.Start();
    }

    private void Update()
    {
        _dialogueObject?.Close();

        if (!_dialogues.Any())
            return;

        var playerInstance = PlayerInstance;
        if (playerInstance is null || !playerInstance.IsValid())
            return;

        var dialogues = _dialogues.Values;

        var firstDialogue = _dialogues.First().Value;
        var color = dialogues.All(x => x.Color.Equals(firstDialogue.Color)) ? firstDialogue.Color : Color.White;
        var message = string.Join(" | ", dialogues.Select(x => x.Text));

        _dialogueObject = _game.CreateDialogue(message, color, playerInstance, string.Empty, 60 * 1000, false);

        // from wiki: 	Display duration. 0=infinite duration (until closed). -1=based on text length.
        // but if i set duration 0, dialogue text won't show
        // if i set int.MaxValue, dialogue will be closed after ~3s
        // idk why, so i set 1 minute
        // which is bad, because i know that no dialogue in luckyblocks shows too long
        // and i can think about 1 minute dialogue as 'infinite dialogue'
        // but i have no idea how to fix it in right way
    }
}