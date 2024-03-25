using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Wayback;

internal class LightWaybackObject : IWaybackObject
{
    public IObject Object { get; private set; }
    
    private readonly string _name;
    private readonly Vector2 _worldPosition;
    private readonly float _angle;
    private readonly int _direction;
    private readonly Vector2 _linearVelocity;
    private readonly float _angularVelocity;
    private readonly float _health;
    private readonly bool _isBurning;

    public LightWaybackObject(IObject @object)
    {
        Object = @object;
        _name = @object.Name;
        _worldPosition = @object.GetWorldPosition();
        _angle = @object.GetAngle();
        _direction = @object.GetFaceDirection();
        _linearVelocity = @object.GetLinearVelocity();
        _angularVelocity = @object.GetAngularVelocity();
        _health = @object.GetHealth();
        _isBurning = @object.IsBurning;
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
    }

    protected virtual IObject Respawn(IGame game, Vector2 position, int direction)
    {
        return game.CreateObject(_name, position, _angle, _linearVelocity, _angularVelocity, direction);
    }

    protected virtual bool IsValid(IObject @object) => @object.IsValid();
}