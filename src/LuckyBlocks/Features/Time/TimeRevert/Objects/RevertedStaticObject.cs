using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedStaticObject : IRevertedObject
{
    public int OldObjectId { get; }
    public string Name { get; }
    public IObject Object { get; private set; }

    protected virtual bool ForceRemove => false;

    private readonly Vector2 _worldPosition;
    private readonly int _direction;
    private readonly float _angle;
    private readonly Vector2 _linearVelocity;
    private readonly float _angularVelocity;
    private readonly string _customId;
    private readonly bool _isBurning;
    private readonly BodyType _bodyType;

    public RevertedStaticObject(IObject @object)
    {
        Object = @object;
        OldObjectId = Object.UniqueId;
        Name = @object.Name;
        _customId = @object.CustomId;
        _worldPosition = @object.GetWorldPosition();
        _angle = @object.GetAngle();
        _direction = @object.GetFaceDirection();
        _linearVelocity = @object.GetLinearVelocity();
        _angularVelocity = @object.GetAngularVelocity();
        _isBurning = @object.IsBurning;
        _bodyType = @object.GetBodyType();
    }

    void IRevertedEntity.Restore(IGame game) => Restore(game);

    public int Restore(IGame game, Dictionary<int, int>? objectsMap = null)
    {
        if (ForceRemove)
        {
            Object.Remove();
        }

        if (!Object.IsValid())
        {
            Object = Respawn(game);

            if (Object is null)
                return 0;
        }
        else
        {
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
        }

        if (Object.CustomId != _customId)
        {
            Object.CustomId = _customId;
        }

        if (_isBurning)
        {
            Object.SetMaxFire();
        }
        else
        {
            Object.ClearFire();
        }

        if (Object.GetBodyType() != _bodyType)
        {
            Object.SetBodyType(_bodyType);
        }

        RestoreInternal(game, objectsMap);

        return Object.UniqueId;
    }

    protected virtual void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
    }

    protected List<IObject> MapObjects(IEnumerable<int> oldObjectIds, Dictionary<int, int> objectsMap, IGame game)
    {
        return oldObjectIds
            .Select(x => (x, game.GetObject(x)))
            .ToDictionary(x => x.Item1, x => x.Item2)
            .Select(x =>
                x.Value ?? (objectsMap.TryGetValue(x.Key, out var mappedObject) ? game.GetObject(mappedObject) : null))
            .Where(x => x is not null)
            .ToList()!;
    }

    protected virtual IObject? Respawn(IGame game)
    {
        return game.CreateObject(Name, _worldPosition, _angle, _linearVelocity, _angularVelocity, _direction);
    }
}