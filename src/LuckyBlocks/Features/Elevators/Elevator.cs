using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Elevators;

internal record Elevator(
    IObject ElevatorObject,
    IObjectElevatorAttachmentJoint AttachmentJoint,
    List<IObjectElevatorPathJoint> PathJoints,
    bool IsAuto)
{
    public float LastArrivalTime { get; private set; }

    public void Arrive(float elapsedGameTime)
    {
        LastArrivalTime = elapsedGameTime;
    }
}