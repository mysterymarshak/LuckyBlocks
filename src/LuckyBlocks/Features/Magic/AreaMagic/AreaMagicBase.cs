using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal abstract class AreaMagicBase : MagicBase, IAreaMagic
{
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

    protected Player Wizard { get; }

    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly Dictionary<Area, List<IObject>> _objectsByArea = new();

    protected AreaMagicBase(Player wizard, BuffConstructorArgs args, int direction = default) : base(args)
        => (Wizard, _effectsPlayer, Direction, _game) = (wizard, args.EffectsPlayer, direction, args.Game);

    public void Reflect()
    {
        Direction = -Direction;
    }

    public void Cast(Area fullArea, Area iterationArea)
    {
        CastInternal(fullArea, iterationArea);
    }

    public abstract void PlayEffects(Area area);

    protected abstract void CastInternal(Area fullArea, Area iterationArea);

    protected IEnumerable<IObject> GetAffectedObjectsByArea(Area fullArea, Area iterationArea)
    {
        if (!_objectsByArea.TryGetValue(fullArea, out var objects))
        {
            objects = _game.GetObjectsByArea(fullArea).ToList();
            _objectsByArea.Add(fullArea, objects);
        }

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
}