using System;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs;

internal abstract class FinishableBuffBase : IFinishableBuff
{
    public abstract string Name { get; }

    public IFinishCondition<IFinishableBuff> WhenFinish => _finishCondition;

    protected abstract Color BuffColor { get; }
    protected Player Player { get; }
    protected ILifetimeScope LifetimeScope { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private readonly INotificationService _notificationService;
    private readonly BuffFinishCondition _finishCondition;

    private int _dialogueId;

    protected FinishableBuffBase(Player player, INotificationService notificationService, ILifetimeScope lifetimeScope)
    {
        Player = player;
        _notificationService = notificationService;
        LifetimeScope = lifetimeScope.BeginLifetimeScope();
        ExtendedEvents = LifetimeScope.Resolve<IExtendedEvents>();
        _finishCondition = new();
    }

    public abstract void Run();
    public abstract void ExternalFinish();

    protected void ShowDialogue(string message, Color color, TimeSpan displayTime, IPlayer? player = default,
        bool ignoreDeath = false, bool realTime = false)
    {
        CloseDialogue();

        if ((player, Player.Instance) is (null, null))
            return;

        _dialogueId =
            _notificationService.CreateDialogueNotification(message, color, displayTime, player ?? Player.Instance!,
                ignoreDeath, realTime);
    }

    protected void CloseDialogue()
    {
        CloseDialogue(Player.Instance);
    }

    protected void CloseDialogue(IPlayer? player)
    {
        if (player is null)
            return;

        _notificationService.RemoveDialogue(player, _dialogueId);
    }

    protected void SendFinishNotification()
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