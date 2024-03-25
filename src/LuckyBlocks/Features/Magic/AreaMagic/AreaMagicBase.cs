using System;
using System.Collections.Generic;
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
            if (_direction != default)
                return _direction;
            
            var playerInstance = Wizard.Instance;
            ArgumentWasNullException.ThrowIfNull(playerInstance);

            return _direction = playerInstance.GetFaceDirection();
        }
        private set => _direction = value;
    }

    protected Player Wizard { get; }
    
    private const float AREA_GROW_EXTENT = 2f;
    
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;

    private int _direction;

    protected AreaMagicBase(Player wizard, BuffConstructorArgs args, int direction = default) : base(args)
        => (Wizard, _effectsPlayer, Direction, _game) = (wizard, args.EffectsPlayer, direction, args.Game);

    public void Reflect()
    {
        Direction = -Direction;
    }

    public void Cast(Area area)
    {
        CastInternal(area);
    }

    public abstract void PlayEffects(Area area);

    protected abstract void CastInternal(Area area);

    protected IEnumerable<IObject> GetObjectsByArea(Area area)
    {
        area.Grow(AREA_GROW_EXTENT);
        return _game.GetObjectsByArea(area);
    }

    protected IEnumerable<FireNode> GetFireNodesByArea(Area area)
    {
        area.Grow(AREA_GROW_EXTENT);
        return _game.GetFireNodes(area);
    }

    protected void PlayEffects(string effectName, Area area, int direction)
    {
        _effectsPlayer.PlayEffect(effectName, direction == 1 ? area.TopRight : area.TopLeft);
        _effectsPlayer.PlayEffect(effectName,
            direction == 1 ? ((area.BottomRight + area.TopRight) / 2) : ((area.BottomLeft + area.TopLeft) / 2));
        _effectsPlayer.PlayEffect(effectName, direction == 1 ? area.BottomRight : area.BottomLeft);
    }
}