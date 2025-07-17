using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Triggers;
using LuckyBlocks.Utils.Watchers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Elevators;

internal interface IElevatorsService
{
    void Initialize();
    Elevator? GetElevator(IObjectElevatorAttachmentJoint attachmentJoint);
}

internal class ElevatorsService : IElevatorsService
{
    private readonly ITriggersService _triggersService;
    private readonly IObjectsWatcher _objectsWatcher;
    private readonly ITimeProvider _timeProvider;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly List<Elevator> _elevators = [];

    public ElevatorsService(ITriggersService triggersService, IObjectsWatcher objectsWatcher,
        ITimeProvider timeProvider, IGame game, ILogger logger)
    {
        _triggersService = triggersService;
        _objectsWatcher = objectsWatcher;
        _timeProvider = timeProvider;
        _game = game;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            var elevatorAttachmentJoints = _game.GetObjects<IObjectElevatorAttachmentJoint>();
            foreach (var attachmentJoint in elevatorAttachmentJoints)
            {
                var pathJoints = _objectsWatcher.StaticObjects.Values
                    .Where(x => x.CustomId.StartsWith(
                        $"{attachmentJoint.CustomId.Split(["_EleJoint"], StringSplitOptions.None)[0]}_PathJoint_"))
                    .Cast<IObjectElevatorPathJoint>()
                    .ToList();

                var elevator = new Elevator(attachmentJoint.GetTargetObject(), attachmentJoint, pathJoints,
                    IsAutoElevator(attachmentJoint));

                var hookTrigger = _triggersService.CreateHookForObject(attachmentJoint, OnElevatorArrived);
                attachmentJoint.AddActivateTriggerOnDestination(hookTrigger);

                _elevators.Add(elevator);
                _logger.Debug("Elevator '{Elevator}' registered", elevator);
            }
        }
        catch (Exception exception)
        {
#if DEBUG
            _logger.Error(exception, "Exception in ElevatorsService.Initialize");
#else
            _logger.Warning("Cannot initialize elevators");
#endif
        }
    }

    public Elevator? GetElevator(IObjectElevatorAttachmentJoint attachmentJoint)
    {
        return _elevators.FirstOrDefault(x => x.AttachmentJoint == attachmentJoint);
    }

    private void OnElevatorArrived(TriggerArgs args)
    {
        var attachmentJoint = (IObjectElevatorAttachmentJoint)args.Sender;
        var elevator = GetElevator(attachmentJoint);

        elevator?.Arrive(_timeProvider.ElapsedGameTime);

        _logger.Verbose("Elevator {ElevatorId} arrived at {PathJointId}", attachmentJoint.UniqueId,
            attachmentJoint.GetElevatorPathJoint()?.UniqueId);
    }

    private bool IsAutoElevator(IObjectElevatorAttachmentJoint attachmentJoint)
    {
        return string.IsNullOrWhiteSpace(attachmentJoint.GetOnDestinationReachedMethod());
    }
}