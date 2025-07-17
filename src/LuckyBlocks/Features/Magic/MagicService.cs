using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Features.Time;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic;

internal interface IMagicService
{
    IFinishCondition<IMagic> Cast<T>(T magic) where T : IMagic;
    MagicServiceState CloneState();
    void RestoreState(MagicServiceState state);
}

internal class MagicService : IMagicService
{
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly ITimeProvider _timeProvider;
    private readonly IExtendedEvents _extendedEvents;
    private readonly List<IMagic> _magics = [];
    private readonly List<IAreaMagic> _areaMagics = [];
    private readonly Dictionary<IMagic, IEventSubscription> _deathSubscriptions = new();

    public MagicService(IGame game, ILogger logger, ITimeProvider timeProvider, ILifetimeScope lifetimeScope)
    {
        _game = game;
        _logger = logger;
        _timeProvider = timeProvider;
        var serviceLifetimeScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = serviceLifetimeScope.Resolve<IExtendedEvents>();
    }

    public IFinishCondition<IMagic> Cast<T>(T magic) where T : IMagic
    {
        var wizard = magic.Wizard;

        if (!_magics.Contains(magic))
        {
            switch (magic)
            {
                case IAreaMagic areaMagic:
                    RegisterAreaMagic(areaMagic);
#if DEBUG
                    if (_game.IsEditorTest)
                    {
                        var debugMagicDrawTimer = new PeriodicTimer<IAreaMagic>(TimeSpan.Zero,
                            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop,
                            magic =>
                            {
                                _game.DrawArea(magic.GetCurrentIteration());
                                _game.DrawArea(magic.GetFullArea(), Color.Red);
                            }, x => x.IsFinished, null, areaMagic, _extendedEvents);
                        debugMagicDrawTimer.Start();
                    }
#endif
                    break;
                case INonAreaMagic nonAreaMagic:
                    RegisterNonAreaMagic(nonAreaMagic);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(magic));
            }

            _magics.Add(magic);

            if (magic.IsCloned)
            {
                var buff = wizard.WizardBuff;
                if (buff is null)
                {
                    throw new InvalidOperationException("trynna to cast cloned magic for player which is not wizard");
                }

                buff.BindMagic(magic);
                _logger.Debug("{MagicName} was binded for {WizardName}", magic.Name, wizard.Name);
            }
        }

        if (!magic.IsCloned || (magic.IsCloned && magic.ShouldCastOnRestore))
        {
            magic.Cast();
            _logger.Debug("{MagicName} casted by {WizardName}", magic.Name, wizard.Name);
        }

        return magic.WhenFinish;
    }

    public MagicServiceState CloneState()
    {
        var clonedMagics = _magics
            .Select(x => x.Clone())
            .ToList();

#if DEBUG
        _logger.Debug("Cloned magics: {ClonedMagics}", clonedMagics.Select(x => $"{x.Wizard.Name}: {x.Name}").ToList());
#endif

        return new MagicServiceState(clonedMagics);
    }

    public void RestoreState(MagicServiceState state)
    {
        RemoveAllMagics();

        foreach (var magic in state.Magics)
        {
            Cast(magic);
            magic.OnRestored();
            _logger.Debug("{MagicName} restored for {WizardName}", magic.Name, magic.Wizard.Name);
        }

        _logger.Debug("Magic service state restored");
    }

    private void RegisterAreaMagic(IAreaMagic areaMagic)
    {
        _areaMagics.Add(areaMagic);

        areaMagic.Iterate += OnAreaMagicIterated;
        HookEvents(areaMagic);
    }

    private void RegisterNonAreaMagic(INonAreaMagic nonAreaMagic)
    {
        HookEvents(nonAreaMagic);
    }

    private void HookEvents(IMagic magic)
    {
        var wizard = magic.Wizard;
        var wizardInstance = wizard.Instance!;

        var subscription =
            _extendedEvents.HookOnDead(wizardInstance, (Event _) => StopMagic(magic), EventHookMode.Default);
        _deathSubscriptions.Add(magic, subscription);

        magic.WhenFinish
            .Invoke(OnMagicFinished);
    }

    private void StopMagic(IMagic magic)
    {
        magic.ExternalFinish();
    }

    private void RemoveAllMagics()
    {
        for (var index = _magics.Count - 1; index >= 0; index--)
        {
            var magic = _magics[index];
            StopMagic(magic);
        }
    }

    private void OnAreaMagicIterated(IAreaMagic areaMagic)
    {
        var iterationArea = areaMagic.GetCurrentIteration();
        var fullArea = areaMagic.GetFullArea();

        areaMagic.PlayEffects(iterationArea);

        if (_timeProvider.IsTimeStopped)
            return;

        areaMagic.Cast(fullArea, iterationArea);
        CheckCollisions();
    }

    private void OnMagicFinished(IMagic magic)
    {
        var deathSubscription = _deathSubscriptions[magic];
        _deathSubscriptions.Remove(magic);
        deathSubscription.Dispose();

        _magics.Remove(magic);
        if (magic is IAreaMagic areaMagic)
        {
            areaMagic.Iterate -= OnAreaMagicIterated;
            _areaMagics.Remove(areaMagic);
        }

        _logger.Debug("{MagicName} removed", magic.Name);
    }

    private void CheckCollisions()
    {
        if (_areaMagics.Count <= 1)
            return;

        for (var i = 0; i < _areaMagics.Count; i++)
        {
            var magic1 = _areaMagics[i];
            var magic1Zone = magic1.GetCurrentIteration();

            for (var j = i + 1; j < _areaMagics.Count; j++)
            {
                var magic2 = _areaMagics[j];
                var magic2Zone = magic2.GetCurrentIteration();

                var intersectArea = Area.Intersect(magic1Zone, magic2Zone);
                intersectArea = intersectArea.IsEmpty
                    ? magic1Zone.Contains(magic2Zone) ? magic1Zone :
                    magic2Zone.Contains(magic1Zone) ? magic2Zone : intersectArea
                    : intersectArea;
                if (intersectArea.IsEmpty)
                    continue;

                var collisionResult = Collide(magic1, magic2);
                if (collisionResult == MagicCollisionResult.None)
                    continue;

                HandleMagicCollide(magic1, magic2, intersectArea, collisionResult);

                _logger.Debug("{Magic1Name} and {Magic2Name} was collided with result '{CollisionResult}'",
                    magic1.Name, magic2.Name, collisionResult);
            }
        }
    }

    private void HandleMagicCollide(IAreaMagic magic1, IAreaMagic magic2, Area intersectArea,
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
}