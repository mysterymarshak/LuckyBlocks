using System;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs;

internal abstract class FinishableBuffBase : IFinishableBuff
{
    public abstract string Name { get; }
    public bool IsFinished { get; private set; }

    public IFinishCondition<IFinishableBuff> WhenFinish => _finishCondition;

    protected abstract Color BuffColor { get; }
    protected virtual Color ChatColor => BuffColor;
    protected Player Player { get; }
    protected IPlayer? PlayerInstance => Player.Instance;
    protected IExtendedEvents ExtendedEvents { get; }

    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly BuffFinishCondition _finishCondition;

    private int _dialogueId;
    private bool _lastDialogueIgnoresFinish;

    protected FinishableBuffBase(Player player, BuffConstructorArgs args)
    {
        Player = player;
        _notificationService = args.NotificationService;
        _logger = args.Logger;
        _lifetimeScope = args.LifetimeScope.BeginLifetimeScope();
        ExtendedEvents = _lifetimeScope.Resolve<IExtendedEvents>();
        _finishCondition = new BuffFinishCondition();
    }

    public abstract void Run();

    public void ExternalFinish()
    {
        InternalFinish();
    }

    protected virtual void OnFinishInternal()
    {
    }

    protected void ShowDialogue(string message, TimeSpan displayTime, Color? color = null,
        IPlayer? givenPlayerInstance = null,
        bool ignoreDeath = false, bool realTime = false, bool ignoreFinish = false)
    {
        var playerInstance = givenPlayerInstance ?? PlayerInstance;

        CloseDialogue(playerInstance);

        _dialogueId =
            _notificationService.CreateDialogueNotification(message, color ?? BuffColor, displayTime, playerInstance!,
                ignoreDeath,
                realTime);
        _lastDialogueIgnoresFinish = ignoreFinish;
    }

    protected void ShowChatMessage(string message, Color? color = null)
    {
        _notificationService.CreateChatNotification(message, color ?? ChatColor, Player.UserIdentifier);
    }

    protected void CloseDialogue()
    {
        CloseDialogue(PlayerInstance);
    }

    protected void CloseDialogue(IPlayer? playerInstance)
    {
        if (playerInstance is null)
            return;

        _notificationService.RemoveDialogue(playerInstance, _dialogueId);
    }

    protected void InternalFinish()
    {
        if (IsFinished)
            return;

        try
        {
            if (!_lastDialogueIgnoresFinish)
            {
                CloseDialogue();
            }

            OnFinishInternal();

            _lifetimeScope.Dispose();
            ExtendedEvents.Clear();

            IsFinished = true;

            SendFinishNotification();
        }
        catch (Exception exception)
        {
            _logger.Error(exception,
                "unexpected exception in finishable buff {BuffName} OnFinish for player {PlayerName}", Name,
                Player.Name);
        }
    }

    private void SendFinishNotification()
    {
        _finishCondition.Callbacks?.Invoke(this);
        _finishCondition.Dispose();
    }

    private class BuffFinishCondition : IFinishCondition<IFinishableBuff>
    {
        public Action<IFinishableBuff>? Callbacks { get; private set; }

        public IFinishCondition<IFinishableBuff> Invoke(Action<IFinishableBuff> callback)
        {
            Callbacks += callback;
            return this;
        }

        public void Dispose()
        {
            Callbacks = null;
        }
    }
}