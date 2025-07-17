using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedOnDestroyedTrigger : RevertedTrigger
{
    protected override bool ForceRemove { get; }
    // idk why but this trigger fired only once and i cant fix that without recreating

    private readonly IEnumerable<int> _oldObjectIds;

    public RevertedOnDestroyedTrigger(IObjectOnDestroyedTrigger onDestroyedTrigger) : base(onDestroyedTrigger)
    {
        var objects = onDestroyedTrigger.GetTriggerDestroyObjects();
        ForceRemove = objects.Length > 0;
        _oldObjectIds = objects.Select(x => x.UniqueId);
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        base.RestoreInternal(game, objectsMap);
        ArgumentWasNullException.ThrowIfNull(objectsMap);

        var newObjects = MapObjects(_oldObjectIds, objectsMap, game);
        var onDestroyedTrigger = (IObjectOnDestroyedTrigger)Object;

        if (!onDestroyedTrigger.GetTriggerDestroyObjects().SequenceEqual(newObjects))
        {
            onDestroyedTrigger.SetTriggerDestroyObjects(newObjects);
        }
        // onDestroyedTrigger.Reset();
    }
}