using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedChangeBodyTypeTrigger : RevertedTrigger
{
    private readonly IEnumerable<int> _oldObjectIds;
    private readonly BodyType _bodyType;

    public RevertedChangeBodyTypeTrigger(IObjectChangeBodyTypeTrigger changeBodyTypeTrigger) : base(
        changeBodyTypeTrigger)
    {
        _oldObjectIds = changeBodyTypeTrigger
            .GetTargetObjects()
            .Select(x => x.UniqueId);
        _bodyType = changeBodyTypeTrigger.GetTargetBodyType();
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        base.RestoreInternal(game, objectsMap);
        ArgumentWasNullException.ThrowIfNull(objectsMap);

        var newObjects = MapObjects(_oldObjectIds, objectsMap, game);
        var changeBodyTypeTrigger = (IObjectChangeBodyTypeTrigger)Object;

        if (!changeBodyTypeTrigger.GetTargetObjects().SequenceEqual(newObjects))
        {
            changeBodyTypeTrigger.SetTargetObjects(newObjects);
        }

        if (changeBodyTypeTrigger.GetTargetBodyType() != _bodyType)
        {
            changeBodyTypeTrigger.SetTargetBodyType(_bodyType);
        }
    }
}