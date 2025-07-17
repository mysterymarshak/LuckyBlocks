using System.Collections.Generic;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedDynamicObject : IRevertedObject
{
    public int OldObjectId { get; }
    public string Name { get; }
    public IObject Object { get; protected set; }

    protected Vector2 WorldPosition { get; }
    protected int Direction { get; }
    protected float Angle { get; }
    protected Vector2 LinearVelocity { get; }
    protected float AngularVelocity { get; }

    private readonly string _customId;
    private readonly BodyType _bodyType;
    private readonly float _health;
    private readonly bool _isBurning;
    private readonly Point _sizeFactor;
    private readonly bool _isMissile;
    private readonly bool _stickyFeet;

    public RevertedDynamicObject(IObject @object)
    {
        Object = @object;
        OldObjectId = Object.UniqueId;
        Name = @object.Name;
        _customId = @object.CustomId;
        WorldPosition = @object.GetWorldPosition();
        Angle = @object.GetAngle();
        Direction = @object.GetFaceDirection();
        LinearVelocity = @object.GetLinearVelocity();
        AngularVelocity = @object.GetAngularVelocity();
        _bodyType = @object.GetBodyType();
        _health = @object.GetHealth();
        _isBurning = @object.IsBurning;
        _sizeFactor = @object.GetSizeFactor();
        _isMissile = @object.IsMissile;
        _stickyFeet = @object.GetStickyFeet();
    }

    void IRevertedEntity.Restore(IGame game) => Restore(game);

    public int Restore(IGame game, Dictionary<int, int>? objectsMap = null)
    {
        if (!IsValid(Object))
        {
            Object = Respawn(game);

            if (Object is null)
                return 0;
        }
        else
        {
            if (Object.GetWorldPosition() != WorldPosition)
            {
                Object.SetWorldPosition(WorldPosition);
            }

            if (Object.GetAngle() != Angle)
            {
                Object.SetAngle(Angle);
            }

            if (Object.GetLinearVelocity() != LinearVelocity)
            {
                Object.SetLinearVelocity(LinearVelocity);
            }

            if (Object.GetAngularVelocity() != AngularVelocity)
            {
                Object.SetAngularVelocity(AngularVelocity);
            }

            if (Object.GetFaceDirection() != Direction)
            {
                Object.SetFaceDirection(Direction);
            }
        }

        RestoreInternal(game, objectsMap);

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

        if (_isBurning)
        {
            Object.SetMaxFire();
        }
        else
        {
            Object.ClearFire();
        }

        if (Object.GetSizeFactor() != _sizeFactor)
        {
            Object.SetSizeFactor(_sizeFactor);
        }

        if (Object.IsMissile != _isMissile)
        {
            Object.TrackAsMissile(_isMissile);
        }

        if (Object.GetStickyFeet() != _stickyFeet)
        {
            Object.SetStickyFeet(_stickyFeet);
        }

        return Object.UniqueId;
    }

    protected virtual void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
    }

    protected virtual IObject? Respawn(IGame game)
    {
        return game.CreateObject(Name, WorldPosition, Angle, LinearVelocity, AngularVelocity, Direction);
    }

    protected virtual bool IsValid(IObject @object)
    {
        return @object.IsValid();
    }
}