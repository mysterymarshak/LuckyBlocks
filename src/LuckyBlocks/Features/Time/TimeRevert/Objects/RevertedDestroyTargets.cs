using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedDestroyTargets : RevertedStaticObject
{
    private readonly IEnumerable<int> _triggerObjectIds;
    private readonly IEnumerable<int> _destroyObjectIds;
    private readonly bool _destroyObjectsExist;
    private readonly int _destroyCount;
    private readonly int _delay;

    public RevertedDestroyTargets(IObjectDestroyTargets destroyTargets) : base(destroyTargets)
    {
        _triggerObjectIds = destroyTargets
            .GetTriggerDestroyObjects()
            .Select(x => x.UniqueId);
        var destroyObjects = destroyTargets.GetObjectsToDestroy();
        _destroyObjectsExist = destroyObjects.Length > 0;
        _destroyObjectIds = destroyObjects.Select(x => x.UniqueId);
        _destroyCount = destroyTargets.GetTriggerDestroyCount();
        _delay = destroyTargets.GetDelay();
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        ArgumentWasNullException.ThrowIfNull(objectsMap);

        if (!_destroyObjectsExist)
            return;

        var newTriggerObjects = MapObjects(_triggerObjectIds, objectsMap, game);
        var newDestroyObjects = MapObjects(_destroyObjectIds, objectsMap, game);
        var destroyTargets = (IObjectDestroyTargets)Object;

        if (!destroyTargets.GetTriggerDestroyObjects().SequenceEqual(newTriggerObjects))
        {
            destroyTargets.SetTriggerDestroyObjects(newTriggerObjects);
        }

        if (!destroyTargets.GetObjectsToDestroy().SequenceEqual(newDestroyObjects))
        {
            destroyTargets.SetObjectsToDestroy(newDestroyObjects);
        }

        if (destroyTargets.GetTriggerDestroyCount() != _destroyCount)
        {
            destroyTargets.SetTriggerDestroyCount(_destroyCount);
        }

        if (destroyTargets.GetDelay() != _delay)
        {
            destroyTargets.SetDelay(_delay);
        }
    }

    protected override IObject? Respawn(IGame game)
    {
        return _destroyObjectsExist ? base.Respawn(game) : null;
    }
}