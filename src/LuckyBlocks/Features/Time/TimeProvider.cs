using Autofac;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time;

internal interface ITimeProvider
{
    bool IsTimeStopped { get; }
    float ElapsedFromPreviousUpdate { get; }
    float TimeModifier { get; }
    float GameSlowMoModifier { get; }
    void Initialize();
}

internal class TimeProvider : ITimeProvider
{
    public bool IsTimeStopped => _timeStopService.IsTimeStopped;
    public float ElapsedFromPreviousUpdate { get; private set; }
    public float TimeModifier => GameSlowMoModifier * (_timeStopService.IsTimeStopped ? 0 : 1);
    public float GameSlowMoModifier => _game.SlowmotionModifier;

    private readonly ITimeStopService _timeStopService;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;

    public TimeProvider(ITimeStopService timeStopService, IGame game, ILifetimeScope lifetimeScope)
        => (_timeStopService, _game, _extendedEvents) = (timeStopService, game,
            lifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>());

    public void Initialize()
    {
        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.GlobalSharedPre);
    }

    private bool OnUpdate(Event<float> @event)
    {
        ElapsedFromPreviousUpdate = @event.Args * TimeModifier;

        return false;
    }
}