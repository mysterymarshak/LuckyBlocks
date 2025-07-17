using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedDestroyTrigger : RevertedTrigger
{
    private readonly IEnumerable<int> _oldObjectIds;

    public RevertedDestroyTrigger(IObjectDestroyTrigger destroyTrigger) : base(destroyTrigger)
    {
        _oldObjectIds = destroyTrigger
            .GetObjectsToDestroy()
            .Select(x => x.UniqueId);
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        base.RestoreInternal(game, objectsMap);
        ArgumentWasNullException.ThrowIfNull(objectsMap);

        var newObjects = MapObjects(_oldObjectIds, objectsMap, game);
        var destroyTrigger = (IObjectDestroyTrigger)Object;

        if (!destroyTrigger.GetObjectsToDestroy().SequenceEqual(newObjects))
        {
            destroyTrigger.SetObjectsToDestroy(newObjects);
        }
    }
}