using System.Diagnostics;
using Autofac;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time;

internal class DebugTimeProvider : ITimeProvider
{
    public Stopwatch Stopwatch { get; }
    public float ElapsedGameTime { get; private set; }
    public float ElapsedRealTime => _game.TotalElapsedRealTime;
    public bool IsTimeStopped => _timeStopService.IsTimeStopped;
    public float ElapsedFromPreviousUpdate { get; private set; }
    public float TimeModifier => GameSlowMoModifier * (_timeStopService.IsTimeStopped ? 0 : 1);
    public float GameSlowMoModifier => _game.SlowmotionModifier;

    private readonly ITimeStopService _timeStopService;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly IExtendedEvents _extendedEvents;

    public DebugTimeProvider(ITimeStopService timeStopService, IGame game, ILogger logger, ILifetimeScope lifetimeScope)
    {
        _timeStopService = timeStopService;
        _game = game;
        _logger = logger;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        Stopwatch = new Stopwatch();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.GlobalSharedPre);
        _extendedEvents.HookOnUpdate(OnUpdatePost, EventHookMode.GlobalSharedPost);
    }

    private bool OnUpdate(Event<float> @event)
    {
        ElapsedFromPreviousUpdate = @event.Args * TimeModifier;
        ElapsedGameTime += ElapsedFromPreviousUpdate;

        if (!Stopwatch.IsRunning)
        {
            Stopwatch.Restart();
        }

        return false;
    }

    private void OnUpdatePost(Event<float> @event)
    {
        Stopwatch.Stop();
        var elapsed = Stopwatch.ElapsedMilliseconds;

        if (elapsed > 16)
        {
            _logger.Warning("Long update! Took {Elapsed}ms", elapsed);
        }
    }
}