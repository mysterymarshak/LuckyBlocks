using System;
using System.Collections.Generic;
using LuckyBlocks.Features.Elevators;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedElevatorAttachmentJoint : RevertedStaticObject
{
    private readonly IObjectElevatorPathJoint? _pathJoint;
    private readonly IObjectElevatorPathJoint? _nextPathJoint;
    private readonly float _motorSpeed;
    private readonly Elevator? _elevator;
    private readonly float _lastArrivalTime;
    private readonly float _elapsedGameTime;

    public RevertedElevatorAttachmentJoint(IObjectElevatorAttachmentJoint attachmentJoint,
        IElevatorsService elevatorsService, ITimeProvider timeProvider) : base(attachmentJoint)
    {
        _pathJoint = attachmentJoint.GetElevatorPathJoint();
        _nextPathJoint = _pathJoint.GetNextPathJoint();
        _motorSpeed = attachmentJoint.GetMotorSpeed();
        _elevator = elevatorsService.GetElevator(attachmentJoint);
        _lastArrivalTime = _elevator?.LastArrivalTime ?? 0;
        _elapsedGameTime = timeProvider.ElapsedGameTime;
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        var attachmentJoint = (IObjectElevatorAttachmentJoint)Object;

        if (_pathJoint is null || _nextPathJoint is null)
            return;

        if (_elevator is not null)
        {
            if (!_elevator.IsAuto)
            {
                foreach (var pathJoint in _elevator.PathJoints)
                {
                    pathJoint.SetNextPathJoint(null);
                }
            }
            else
            {
                var stayedFor = TimeSpan.FromMilliseconds(_elapsedGameTime - _lastArrivalTime);
                var stayDelay = TimeSpan.FromMilliseconds(_pathJoint.GetDelay());
                if (stayedFor < stayDelay - TimeSpan.FromMilliseconds(100))
                {
                    var delayLeft = stayDelay - stayedFor;
                    _pathJoint.SetDelay((int)delayLeft.TotalMilliseconds);
                    _nextPathJoint.SetDelay((int)delayLeft.TotalMilliseconds);

                    Awaiter.Start(delegate
                    {
                        _pathJoint.SetDelay((int)stayDelay.TotalMilliseconds);
                        _nextPathJoint.SetDelay((int)stayDelay.TotalMilliseconds);
                    }, 2);
                }
            }
        }

        _pathJoint.SetNextPathJoint(_nextPathJoint);
        attachmentJoint.SetElevatorPathJoint(_pathJoint);
        attachmentJoint.SetMotorSpeed(_motorSpeed);
    }
}