using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal abstract class DurableBuffBase : FinishableBuffBase, IDurableBuff
{
    public abstract TimeSpan Duration { get; }
    public TimeSpan TimeLeft => _timer?.TimeLeft ?? TimeSpan.FromMilliseconds(_duration);

    protected bool IsCloned { get; private set; }

    private readonly ILogger _logger;

    private float _duration;
    private Timer? _timer;

    protected DurableBuffBase(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args)
    {
        _logger = args.Logger;
        _duration = timeLeft == default ? (float)Duration.TotalMilliseconds : (float)timeLeft.TotalMilliseconds;
    }

    public IDurableBuff Clone(Player? player = null)
    {
        var clonedBuff = CloneInternal(player ?? Player);
        clonedBuff.IsCloned = true;
        return clonedBuff;
    }

    public sealed override void Run()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        CreateAndStartTimer();
        OnRunInternal();
        WhenFinish.Invoke(StopTimer);
    }

    public void ApplyAgain(IBuff additionalBuff)
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        var timeLeft = TimeLeft;
        var additionalBuffDuration = ((IDurableBuff)additionalBuff).TimeLeft;
        _duration = MathHelper.Clamp((float)(TimeLeft + additionalBuffDuration).TotalMilliseconds,
            (float)TimeLeft.TotalMilliseconds,
            (float)Duration.TotalMilliseconds);

        if (Math.Abs(timeLeft.TotalMilliseconds - _duration) < 1f)
            return;

        CreateAndStartTimer();
        OnApplyAgainInternal();
    }

    protected abstract DurableBuffBase CloneInternal(Player player);
    protected abstract void OnRunInternal();
    protected abstract void OnApplyAgainInternal();

    protected void ShowPersistentDialogue(string message, TimeSpan? displayTime = null, Color? color = null,
        IPlayer? givenPlayerInstance = null,
        bool ignoreDeath = false, bool realTime = false)
    {
        ShowDialogue(message, displayTime ?? TimeLeft, color ?? BuffColor, givenPlayerInstance ?? PlayerInstance,
            ignoreDeath, realTime);
    }

    private void CreateAndStartTimer()
    {
        _timer?.Stop();

        _timer = new Timer(TimeSpan.FromMilliseconds(_duration), TimeBehavior.TimeModifier, InternalFinish,
            ExtendedEvents);
        _timer.Start();
    }

    private void StopTimer(IFinishableBuff buff)
    {
        _timer?.Stop();
    }
}