using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedWeldJoint : RevertedStaticObject
{
    private readonly IEnumerable<int> _oldTargetObjectIds;

    public RevertedWeldJoint(IObjectWeldJoint weldJoint) : base(weldJoint)
    {
        _oldTargetObjectIds = weldJoint
            .GetTargetObjects()
            .Select(x => x.UniqueId);
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        ArgumentWasNullException.ThrowIfNull(objectsMap);

        var newObjects = MapObjects(_oldTargetObjectIds, objectsMap, game);
        var weldJoint = (IObjectWeldJoint)Object;

        if (!weldJoint.GetTargetObjects().SequenceEqual(newObjects))
        {
            weldJoint.SetTargetObjects(newObjects);
        }
    }
}