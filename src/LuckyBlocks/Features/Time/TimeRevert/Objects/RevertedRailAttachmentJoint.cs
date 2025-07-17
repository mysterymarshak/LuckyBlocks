using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedRailAttachmentJoint : RevertedDynamicObject
{
    private readonly IObjectRailJoint _railJoint;
    private readonly float _motorSpeed;
    private readonly bool _motorEnabled;

    public RevertedRailAttachmentJoint(IObjectRailAttachmentJoint railAttachmentJoint) : base(railAttachmentJoint)
    {
        _railJoint = railAttachmentJoint.GetRailJoint();
        _motorSpeed = railAttachmentJoint.GetMotorSpeed();
        _motorEnabled = railAttachmentJoint.GetMotorEnabled();
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        var railAttachmentJoint = (IObjectRailAttachmentJoint)Object;

        if (railAttachmentJoint.GetRailJoint() != _railJoint)
        {
            railAttachmentJoint.SetRailJoint(_railJoint);
        }

        if (railAttachmentJoint.GetMotorSpeed() != _motorSpeed)
        {
            railAttachmentJoint.SetMotorSpeed(_motorSpeed);
        }

        if (railAttachmentJoint.GetMotorEnabled() != _motorEnabled)
        {
            railAttachmentJoint.SetMotorEnabled(_motorEnabled);
        }
    }
}