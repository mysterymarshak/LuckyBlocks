using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Time;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic;

internal interface IMagicService
{
    void Cast(IAreaMagic magic, IPlayer wizardInstance);
}

internal class MagicService : IMagicService
{
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly ITimeProvider _timeProvider;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Dictionary<int, CastingMagic> _castingMagics;

    private int _lastMagicId;

    public MagicService(IGame game, ILogger logger, ITimeProvider timeProvider, ILifetimeScope lifetimeScope)
    {
        _game = game;
        _logger = logger;
        _timeProvider = timeProvider;
        var serviceLifetimeScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = serviceLifetimeScope.Resolve<IExtendedEvents>();
        _castingMagics = new();
    }

    public void Cast(IAreaMagic magic, IPlayer wizardInstance)
    {
        var castingMagic = Cast(magic, wizardInstance.GetWorldPosition());

        _extendedEvents.HookOnDead(wizardInstance, (Event _) => StopMagic(castingMagic), EventHookMode.Default);

        _logger.Debug("{MagicName} casted by {WizardName}", magic.Name, wizardInstance.Name);
    }

    private CastingMagic Cast(IAreaMagic magic, Vector2 startPosition)
    {
        _lastMagicId++;
        var magicId = _lastMagicId;

        var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(magic.PropagationTime.TotalMilliseconds / magic.IterationsCount),
            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop |
            TimeBehavior.TicksInTimeStopDoesntAffectToIterationsCount, () => OnMagicIterated(magicId),
            () => OnMagicFinished(magicId), magic.IterationsCount, _extendedEvents);

        var castingMagic = new CastingMagic(magicId, magic, startPosition, timer);
        _castingMagics.Add(magicId, castingMagic);
        timer.Start();

        return castingMagic;
    }

    private void OnMagicIterated(int magicId)
    {
        var castingMagic = _castingMagics[magicId];
        var magic = castingMagic.Magic;

        if (_timeProvider.IsTimeStopped)
        {
            magic.PlayEffects(castingMagic.GetCurrentIteration());
            return;
        }

        var area = castingMagic.Iterate();
        magic.Cast(area);

        CheckCollisions();
    }

    private void OnMagicFinished(int magicId)
    {
        var castingMagic = _castingMagics[magicId];
        OnMagicFinished(castingMagic);
    }

    private void OnMagicFinished(CastingMagic castingMagic)
    {
        castingMagic.Dispose();
        _castingMagics.Remove(castingMagic.Id);

        _logger.Debug("{CastingMagicName} removed", castingMagic.Name);
    }

    private void CheckCollisions()
    {
        if (_castingMagics.Count <= 1)
            return;

        var castingMagics = _castingMagics.Values;

        for (var i = 0; i < castingMagics.Count; i++)
        {
            var magic1 = castingMagics.ElementAt(i);
            var magic1Zone = magic1.GetCurrentIteration();

            for (var j = i + 1; j < castingMagics.Count; j++)
            {
                var magic2 = castingMagics.ElementAt(j);
                var magic2Zone = magic2.GetCurrentIteration();

                var intersectArea = Area.Intersect(magic1Zone, magic2Zone);
                intersectArea = intersectArea.IsEmpty
                    ? magic1Zone.Contains(magic2Zone) ? magic1Zone :
                    magic2Zone.Contains(magic1Zone) ? magic2Zone : intersectArea
                    : intersectArea;
                if (intersectArea.IsEmpty)
                    continue;

                var collisionResult = Collide(magic1.Magic, magic2.Magic);
                if (collisionResult == MagicCollisionResult.None)
                    continue;

                HandleMagicCollide(magic1, magic2, intersectArea, collisionResult);

                _logger.Debug("{Magic1Name} and {Magic2Name} was collided with result '{CollisionResult}'",
                    magic1.Name, magic2.Name, collisionResult);
            }
        }
    }

    private void HandleMagicCollide(CastingMagic magic1, CastingMagic magic2, Area intersectArea,
        MagicCollisionResult collisionResult)
    {
        if (collisionResult.HasFlag<MagicCollisionResult>(MagicCollisionResult.Absorb))
        {
            StopMagic(magic2);
        }

        if (collisionResult.HasFlag<MagicCollisionResult>(MagicCollisionResult.WasAbsorbed))
        {
            StopMagic(magic1);
        }

        if (collisionResult.HasFlag<MagicCollisionResult>(MagicCollisionResult.Reflect))
        {
            magic2.Reflect();
        }

        if (collisionResult.HasFlag<MagicCollisionResult>(MagicCollisionResult.WasReflected))
        {
            magic1.Reflect();
        }

        if (collisionResult.HasFlag<MagicCollisionResult>(MagicCollisionResult.Explosion))
        {
            _game.TriggerExplosion((intersectArea.TopLeft + intersectArea.TopRight) / 2);
            _game.TriggerExplosion(intersectArea.Center);
            _game.TriggerExplosion((intersectArea.BottomLeft + intersectArea.BottomRight) / 2);

            StopMagic(magic1);
            StopMagic(magic2);
        }
    }

    private MagicCollisionResult Collide(IAreaMagic magic1, IAreaMagic magic2) => (magic1.Type, magic2.Type) switch
    {
        (AreaMagicType.Fire, AreaMagicType.Electric) or (AreaMagicType.Electric, AreaMagicType.Fire)
            => MagicCollisionResult.Explosion,
        (AreaMagicType.Wind, AreaMagicType.Fire) => MagicCollisionResult.Absorb,
        (AreaMagicType.Fire, AreaMagicType.Wind) => MagicCollisionResult.WasAbsorbed,
        _ => MagicCollisionResult.None
    };

    private void StopMagic(CastingMagic castingMagic)
    {
        castingMagic.Stop();
        OnMagicFinished(castingMagic);
    }

    private class CastingMagic
    {
        public string Name => Magic.Name;
        public IAreaMagic Magic { get; }
        public int Id { get; }
        public bool IsFinished { get; private set; }

        private readonly Vector2 _startPosition;
        private readonly PeriodicTimer _timer;

        private int _iterationIndex;

        public CastingMagic(int id, IAreaMagic magic, Vector2 startPosition, PeriodicTimer timer)
            => (Id, Magic, _startPosition, _timer) =
                (id, magic, startPosition, timer);

        public Area Iterate()
        {
            var area = GetCurrentIteration();

            _iterationIndex++;

            return area;
        }

        public void Reflect()
        {
            _iterationIndex = -_iterationIndex;
            Magic.Reflect();
        }

        public Area GetCurrentIteration()
        {
            var direction = Magic.Direction;
            var startOffset = new Vector2(16, 0);
            var zone = GetAllZone();

            var minX = _startPosition.X + (zone.Width / Magic.IterationsCount * _iterationIndex * direction) +
                       (startOffset.X * direction);
            var minY = zone.BottomLeft.Y + startOffset.Y;
            var min = new Vector2(minX, minY);

            var maxX = _startPosition.X + (zone.Width / Magic.IterationsCount * (_iterationIndex + 1) * direction) +
                       (startOffset.X * direction);
            var maxY = zone.TopRight.Y + startOffset.Y;
            var max = new Vector2(maxX, maxY);

            return new Area(min, max);
        }

        public void Dispose()
        {
            Magic.ExternalFinish();
            IsFinished = true;
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private Area GetAllZone()
        {
            var zoneSize = Magic.AreaSize;

            return Magic.Direction == 1
                ? new Area(new Vector2(_startPosition.X, _startPosition.Y), _startPosition + zoneSize)
                : new Area(new Vector2(_startPosition.X - zoneSize.X, _startPosition.Y),
                    new Vector2(_startPosition.X, _startPosition.Y + zoneSize.Y));
        }
    }
}