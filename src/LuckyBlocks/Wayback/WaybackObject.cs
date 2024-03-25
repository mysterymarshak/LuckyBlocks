using System.Linq;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Wayback;

internal class WaybackObject : IWaybackObject
{
    public IObject Object { get; private set; }
    
    private readonly string _name;
    private readonly string _customId;

    // CustomID and CustomId refer to the same property
    // with UniqueID and UniqueId the same situation, so i save only one

    private readonly Vector2 _worldPosition;
    private readonly float _angle;
    private readonly int _direction;
    private readonly Vector2 _linearVelocity;
    private readonly float _angularVelocity;
    private readonly BodyType _bodyType;
    private readonly float _health;
    private readonly bool _isBurning;
    private readonly bool _targetAiEnabled;
    private readonly ObjectAITargetData _objectAiTargetData;
    private readonly Point _sizeFactor;
    private readonly int _animationFrame;
    private readonly bool _isAnimationPaused;
    private readonly bool _isMissile;
    private readonly bool _stickyFeet;
    private readonly float _mass;
    private readonly CollisionFilter _collisionFilter;
    private readonly string[] _colors;

    public WaybackObject(IObject @object)
    {
        Object = @object;
        _name = @object.Name;
        _customId = @object.CustomId;
        _worldPosition = @object.GetWorldPosition();
        _angle = @object.GetAngle();
        _direction = @object.GetFaceDirection();
        _linearVelocity = @object.GetLinearVelocity();
        _angularVelocity = @object.GetAngularVelocity();
        _bodyType = @object.GetBodyType();
        _health = @object.GetHealth();
        _isBurning = @object.IsBurning;
        _targetAiEnabled = @object.GetTargetAIEnabled();
        _objectAiTargetData = @object.GetTargetAIData();
        _sizeFactor = @object.GetSizeFactor();
        _animationFrame = @object.GetAnimationFrame();
        _isAnimationPaused = @object.IsAnimationPaused();
        _isMissile = @object.IsMissile;
        _stickyFeet = @object.GetStickyFeet();
        _mass = @object.GetMass();
        _collisionFilter = @object.GetCollisionFilter();
        _colors = @object.GetColors();
    }

    public virtual void Restore(IGame game)
    {
        if (!IsValid(Object))
        {
            Object = Respawn(game, _worldPosition, _direction);
        }
        
        if (Object.GetWorldPosition() != _worldPosition)
        {
            Object.SetWorldPosition(_worldPosition);
        }

        if (Object.GetAngle() != _angle)
        {
            Object.SetAngle(_angle);
        }

        if (Object.GetLinearVelocity() != _linearVelocity)
        {
            Object.SetLinearVelocity(_linearVelocity);
        }

        if (Object.GetAngularVelocity() != _angularVelocity)
        {
            Object.SetAngularVelocity(_angularVelocity);
        }

        if (Object.GetFaceDirection() != _direction)
        {
            Object.SetFaceDirection(_direction);
        }

        if (Object.CustomId != _customId)
        {
            Object.CustomId = _customId;
        }
        
        if (Object.GetBodyType() != _bodyType)
        {
            Object.SetBodyType(_bodyType);
        }

        if (Object.GetHealth() != _health)
        {
            Object.SetHealth(_health);
        }

        if (Object.IsBurning != _isBurning)
        {
            if (_isBurning)
            {
                Object.SetMaxFire();
            }
            else
            {
                Object.ClearFire();
            }
        }

        if (Object.GetTargetAIEnabled() != _targetAiEnabled)
        {
            Object.SetTargetAIEnabled(_targetAiEnabled);
        }

        if (Object.GetTargetAIData() != _objectAiTargetData)
        {
            Object.SetTargetAIData(_objectAiTargetData);
        }

        if (Object.GetSizeFactor() != _sizeFactor)
        {
            Object.SetSizeFactor(_sizeFactor);
        }

        if (Object.IsAnimationPaused() != _isAnimationPaused)
        {
            if (_isAnimationPaused)
            {
                Object.PauseAnimation();
            }
            else
            {
                Object.PlayAnimation();
            }
        }
        
        if (Object.GetAnimationFrame() != _animationFrame)
        {
            Object.SetAnimationFrame(_animationFrame);
        }

        if (Object.IsMissile != _isMissile)
        {
            Object.TrackAsMissile(_isMissile);
        }

        if (Object.GetStickyFeet() != _stickyFeet)
        {
            Object.SetStickyFeet(_stickyFeet);
        }

        if (Object.GetMass() != _mass)
        {
            Object.SetMass(_mass);
        }

        if (Object.GetCollisionFilter() != _collisionFilter)
        {
            Object.SetCollisionFilter(_collisionFilter);
        }

        if (!Object.GetColors().SequenceEqual(_colors))
        {
            Object.SetColors(_colors);
        }
    }

    protected virtual IObject Respawn(IGame game, Vector2 position, int direction)
    {
        return game.CreateObject(_name, position, _angle, _linearVelocity, _angularVelocity, direction);
    }

    protected virtual bool IsValid(IObject @object) => @object.IsValid();
}