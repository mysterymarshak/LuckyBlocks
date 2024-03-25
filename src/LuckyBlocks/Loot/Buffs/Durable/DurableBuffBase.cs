using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal abstract class DurableBuffBase : FinishableBuffBase, IDurableBuff
{
    public abstract TimeSpan Duration { get; }
    public TimeSpan TimeLeft => _timer?.TimeLeft ?? TimeSpan.FromMilliseconds(_duration);

    protected bool Cloned { get; }
    private readonly ILogger _logger;

    private float _duration;
    private Timer? _timer;

    protected DurableBuffBase(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default,
        bool cloned = false) : base(player, args.NotificationService, args.LifetimeScope) =>
        (_logger, _duration, Cloned) = (args.Logger,
            timeLeft == default ? (float)Duration.TotalMilliseconds : (float)timeLeft.TotalMilliseconds, cloned);
    
    public override void Run()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);
        
        CreateAndStartTimer();
        OnRan();
    }

    public void ApplyAgain(IBuff additionalBuff)
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);
        
        var additionalBuffDuration = ((IDurableBuff)additionalBuff).TimeLeft;
        _duration = MathHelper.Clamp((float)(TimeLeft + additionalBuffDuration).TotalMilliseconds, (float)TimeLeft.TotalMilliseconds,
            (float)Duration.TotalMilliseconds);

        CreateAndStartTimer();
        OnAppliedAgain();
    }

    public override void ExternalFinish()
    {
        CloseDialogue();
        OnFinishInternal();
    }

    public abstract IDurableBuff Clone();

    protected abstract void OnRan();
    protected abstract void OnAppliedAgain();
    protected abstract void OnFinished();

    private void CreateAndStartTimer()
    {
        _timer?.Stop();
        
        _timer = new Timer(TimeSpan.FromMilliseconds(_duration), TimeBehavior.TimeModifier, OnFinishInternal, ExtendedEvents);
        _timer.Start();
    }
    
    private void OnFinishInternal()
    {
        try
        {
            _timer?.Stop();
            LifetimeScope.Dispose();

            OnFinished();
            SendFinishNotification();
        }
        catch (Exception exception)
        {
            _logger.Error(exception,
                "unexpected exception in durable buff {BuffName} OnFinishInternal for player {PlayerName}", Name,
                Player.Name);
        }
    }
}