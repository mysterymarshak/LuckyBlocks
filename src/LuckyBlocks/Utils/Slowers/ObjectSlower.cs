using System;
using System.Threading;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils.Slowers;

internal class ObjectSlower
{
    private Vector2 RealLinearVelocity => _startLinearVelocity + VelocityDisplacement(DeltaTime);
    private float DeltaTime => (_game.TotalElapsedRealTime - _startTime) / 1000;
    
    private const float G = 9.8f;

    private readonly IObject _object;
    private readonly IGame _game;
    private readonly DynamicPeriodicTimer _timer;

    private float _startTime;
    private Vector2 _startLinearVelocity;
    private float _savedAngularVelocity;
    private CancellationToken _cancellationToken;
    private CancellationTokenRegistration _ctr;

    public ObjectSlower(IObject @object, IGame game, TimeSpan sloMoDuration, CancellationToken cancellationToken,
        IExtendedEvents extendedEvents) => (_object, _game, _cancellationToken, _timer) = (@object, game,
        cancellationToken,
        new DynamicPeriodicTimer(TimeSpan.FromMilliseconds(50), TimeSpan.Zero, sloMoDuration, OnTimerTick, default,
            TimeBehavior.RealTime, extendedEvents));

    public void Initialize()
    {
        _startTime = _game.TotalElapsedRealTime;
        _startLinearVelocity = _object.GetLinearVelocity();
        _savedAngularVelocity = _object.GetAngularVelocity();

        _timer.Start();
        _ctr = _cancellationToken.Register(Stop);
    }

    public void Stop()
    {
        _timer.Stop();
        _ctr.Dispose();

        _object.SetLinearVelocity(RealLinearVelocity);
        _object.SetAngularVelocity(_savedAngularVelocity);
    }

    private void OnTimerTick(DynamicPeriodicTimerTickArgs args)
    {
        var linearVelocityStep = _startLinearVelocity / args.StepsCount;
        var angularVelocityStep = _savedAngularVelocity / args.StepsCount;

        _object.SetLinearVelocity(_startLinearVelocity - (linearVelocityStep * args.StepIndex));
        _object.SetAngularVelocity(_savedAngularVelocity - (angularVelocityStep * args.StepIndex));
    }

    private static Vector2 Displacement(Vector2 v, float t)
    {
        var a = new Vector2(0, -G);
        return v * t + a * t * t / 2;
    }

    private static Vector2 VelocityDisplacement(float t)
    {
        var a = new Vector2(0, -G);
        return a * t;
    }
}