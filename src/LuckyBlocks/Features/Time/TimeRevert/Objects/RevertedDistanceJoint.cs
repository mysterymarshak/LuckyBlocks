using System.Collections.Generic;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedDistanceJoint : RevertedStaticObject
{
    private readonly int? _targetJointId;
    private readonly LineVisual _lineVisual;
    private readonly DistanceJointLengthType _lengthType;

    public RevertedDistanceJoint(IObjectDistanceJoint distanceJoint) : base(distanceJoint)
    {
        _targetJointId = distanceJoint.GetTargetObjectJoint()?.UniqueId;
        _lineVisual = distanceJoint.GetLineVisual();
        _lengthType = distanceJoint.GetLengthType();
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        if (_targetJointId is null)
            return;

        var distanceJoint = (IObjectDistanceJoint)Object;

        if (distanceJoint.GetLineVisual() != _lineVisual)
        {
            distanceJoint.SetLineVisual(_lineVisual);
        }

        if (distanceJoint.GetLengthType() != _lengthType)
        {
            distanceJoint.SetLengthType(_lengthType);
        }

        var targetJoint = game.GetObject(_targetJointId.Value);
        if (targetJoint?.IsValid() != true)
        {
            targetJoint = game.GetObject(objectsMap![_targetJointId.Value]);
        }

        if (distanceJoint.GetTargetObjectJoint() != targetJoint)
        {
            distanceJoint.SetTargetObjectJoint((IObjectTargetObjectJoint)targetJoint);
        }
    }
}