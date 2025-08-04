using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal abstract class AreaMagicBase : MagicBase, IAreaMagic
{
    public event Action<IAreaMagic>? Iterate;

    public abstract override string Name { get; }
    public abstract AreaMagicType Type { get; }

    public virtual Vector2 AreaSize => new(100, 45);
    public virtual TimeSpan PropagationTime => TimeSpan.FromMilliseconds(1300);
    public virtual int IterationsCount => 10;

    public int Direction
    {
        get
        {
            if (field != default)
            {
                return field;
            }

            var playerInstance = Wizard.Instance;
            ArgumentWasNullException.ThrowIfNull(playerInstance);

            return field = playerInstance.GetFaceDirection();
        }
        private set;
    }

    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly Dictionary<Area, List<IObject>> _objectsByArea = new();

    private Vector2 _startPosition;
    private PeriodicTimer? _timer;
    private int _iterationIndex;
    private int _iterationsPassed;

    protected AreaMagicBase(Player wizard, MagicConstructorArgs args, int direction = default) : base(wizard, args)
    {
        _effectsPlayer = args.EffectsPlayer;
        _game = args.Game;
        Direction = direction;
    }

    public sealed override IMagic Clone()
    {
        var areaMagic = (AreaMagicBase)base.Clone();
        areaMagic._iterationIndex = _iterationIndex;
        areaMagic._iterationsPassed = _iterationsPassed;
        areaMagic._startPosition = _startPosition;
        areaMagic.Direction = Direction;

        return areaMagic;
    }

    public void Reflect()
    {
        _iterationIndex = -_iterationIndex;
        Direction = -Direction;
    }

    public override void Cast()
    {
        _startPosition = _startPosition == Vector2.Zero ? WizardInstance!.GetWorldPosition() : _startPosition;
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(PropagationTime.TotalMilliseconds / IterationsCount),
            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop |
            TimeBehavior.TicksInTimeStopDoesntAffectToIterationsCount, OnIterate, ExternalFinish,
            IterationsCount - _iterationsPassed,
            ExtendedEvents);
        _timer.Start();
    }

    public virtual void Cast(Area fullArea, Area iterationArea)
    {
        _iterationIndex++;
        _iterationsPassed++;
    }

    public Area GetCurrentIteration()
    {
        var fullArea = GetFullArea();

        var minX = (Direction == 1 ? fullArea.Min.X : fullArea.Max.X) +
                   (fullArea.Width / IterationsCount * (_iterationIndex + 1) * Direction);
        var minY = fullArea.Min.Y;
        var min = new Vector2(minX, minY);

        var maxX = (Direction == 1 ? fullArea.Min.X : fullArea.Max.X) +
                   (fullArea.Width / IterationsCount * (_iterationIndex) * Direction);
        var maxY = fullArea.Max.Y;
        var max = new Vector2(maxX, maxY);

        return new Area(min, max);
    }

    public Area GetFullArea()
    {
        var startOffset = new Vector2(8, -5);
        return Direction == 1
            ? new Area(_startPosition + startOffset, _startPosition + AreaSize + startOffset)
            : new Area(_startPosition + new Vector2(-startOffset.X, startOffset.Y) - new Vector2(AreaSize.X, 0),
                _startPosition + new Vector2(-startOffset.X, startOffset.Y) + new Vector2(0, AreaSize.Y));
    }

    public abstract void PlayEffects(Area area);

    protected IEnumerable<IObject> GetAffectedObjectsByArea(Area fullArea, Area iterationArea)
    {
        var objects = _objectsByArea.GetOrAdd(fullArea, fullArea => _game.GetObjectsByArea(fullArea).ToList());
        return objects.Where(@object =>
        {
            var position = @object.GetWorldPosition();
            if (!fullArea.Contains(position))
                return false;

            var xPosition = position.X;

            if (Direction == 1)
            {
                return xPosition < iterationArea.Max.X;
            }

            return xPosition > iterationArea.Min.X;
        });
    }

    protected IEnumerable<FireNode> GetFireNodesByArea(Area area)
    {
        return _game.GetFireNodes(area);
    }

    protected void PlayEffects(string effectName, Area area, int direction)
    {
        _effectsPlayer.PlayEffect(effectName,
            direction == 1 ? area.TopRight : area.TopLeft - new Vector2(0, area.Height / 4));
        _effectsPlayer.PlayEffect(effectName,
            direction == 1 ? ((area.BottomRight + area.TopRight) / 2) : ((area.BottomLeft + area.TopLeft) / 2));
        _effectsPlayer.PlayEffect(effectName,
            direction == 1 ? area.BottomRight : area.BottomLeft + new Vector2(0, area.Height / 4));
    }

    protected sealed override void OnFinishInternal()
    {
        _timer?.Stop();
    }

    private void OnIterate()
    {
        Iterate?.Invoke(this);
    }
}