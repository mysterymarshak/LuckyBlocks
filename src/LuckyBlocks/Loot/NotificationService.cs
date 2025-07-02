using System;
using System.Threading;
using LuckyBlocks.Features.Chat;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot;

internal interface INotificationService
{
    void CreateChatNotification(string message, Color color);
    void CreateChatNotification(string message, Color color, int userIdentifier);

    int CreateDialogueNotification(string message, Color color, TimeSpan displayTime, IPlayer playerInstance,
        bool ignoreDeath = false, bool realTime = false);

    void RemoveDialogue(IPlayer playerInstance, int id);
    void CreateTextNotification(string message, Color color, TimeSpan displayTime, IPlayer player);
    void CreatePopupNotification(string message, Color color, TimeSpan duration);
    void ClosePopupNotification();
}

internal class NotificationService : INotificationService
{
    private readonly IGame _game;
    private readonly IChat _chat;
    private readonly IDialoguesService _dialoguesService;
    private readonly IEffectsPlayer _effectsPlayer;

    private CancellationTokenSource? _popupMessageCancellationTokenSource;

    public NotificationService(IGame game, IChat chat, IDialoguesService dialoguesService, IEffectsPlayer effectsPlayer)
        => (_game, _chat, _dialoguesService, _effectsPlayer) = (game, chat, dialoguesService, effectsPlayer);

    public void CreateChatNotification(string message, Color color)
    {
        _chat.ShowMessage(message, color);
    }

    public void CreateChatNotification(string message, Color color, int userIdentifier)
    {
        _chat.ShowMessage(message, color, userIdentifier);
    }

    public int CreateDialogueNotification(string message, Color color, TimeSpan displayTime, IPlayer playerInstance,
        bool ignoreDeath = false, bool realTime = false)
    {
        return _dialoguesService.AddDialogue(message, color, displayTime, playerInstance, ignoreDeath, realTime);
    }

    public void RemoveDialogue(IPlayer playerInstance, int id)
    {
        _dialoguesService.RemoveDialogue(playerInstance, id);
    }

    public void CreateTextNotification(string message, Color color, TimeSpan displayTime, IPlayer player)
    {
        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, player.GetWorldPosition(), message, color,
            (float)displayTime.TotalMilliseconds, 1f, true);
        // Text.CreateAnimatedText(message, color, displayTime, player, _game);
    }

    public void CreatePopupNotification(string message, Color color, TimeSpan duration)
    {
        _popupMessageCancellationTokenSource?.Cancel();
        _popupMessageCancellationTokenSource?.Dispose();
        _popupMessageCancellationTokenSource = new CancellationTokenSource();

        _game.ShowPopupMessage(message, color);

        Awaiter.Start(_game.HidePopupMessage, duration, _popupMessageCancellationTokenSource.Token);
    }

    public void ClosePopupNotification()
    {
        _popupMessageCancellationTokenSource?.Cancel();
        _popupMessageCancellationTokenSource?.Dispose();
        _popupMessageCancellationTokenSource = null;

        _game.HidePopupMessage();
    }
}