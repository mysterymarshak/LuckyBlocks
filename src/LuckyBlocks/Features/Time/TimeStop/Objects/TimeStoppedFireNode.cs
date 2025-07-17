using System;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal class TimeStoppedFireNode : ITimeStoppedEntity
{
    public Vector2 Position => _fireNode.Position;

    private readonly FireNode _fireNode;
    private readonly IGame _game;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly RandomPeriodicTimer _timer;

    public TimeStoppedFireNode(FireNode fireNode, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents) => (_fireNode, _game, _effectsPlayer, _timer) = (fireNode, game, effectsPlayer,
        new RandomPeriodicTimer(TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(300), TimeBehavior.RealTime,
            OnTimer, extendedEvents));

    public void Initialize()
    {
        _game.EndFireNode(_fireNode.InstanceID);
        _timer.Start();
    }

    public void ResumeTime()
    {
        _timer.Stop();
        _game.SpawnFireNode(_fireNode.Position, _fireNode.Velocity, FireNodeType.Flamethrower);
    }

    private void OnTimer()
    {
        _effectsPlayer.PlayEffect(EffectName.FireNodeTrailAir, Position);
    }
}