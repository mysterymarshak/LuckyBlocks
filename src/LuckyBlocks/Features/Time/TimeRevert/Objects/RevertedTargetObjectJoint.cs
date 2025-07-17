using System.Collections.Generic;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedTargetObjectJoint : RevertedStaticObject
{
    private readonly int? _targetObjectId;

    public RevertedTargetObjectJoint(IObjectTargetObjectJoint targetObjectJoint) : base(targetObjectJoint)
    {
        _targetObjectId = targetObjectJoint.GetTargetObject()?.UniqueId;
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        if (_targetObjectId is null)
            return;

        var targetObject = game.GetObject(_targetObjectId.Value);
        if (targetObject?.IsValid() != true)
        {
            var targetObjectJoint = (IObjectTargetObjectJoint)Object;
            targetObject = game.GetObject(objectsMap![_targetObjectId.Value]);
            targetObjectJoint.SetTargetObject(targetObject);
        }
    }
}