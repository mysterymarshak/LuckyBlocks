using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils.Slowers;

internal class ProjectileSlower
{
    private readonly IProjectile _projectile;
    private readonly IExtendedEvents _extendedEvents;
    private readonly List<ProjectileTrajectoryNode> _trajectoryNodes;

    private const float SlowMoModifier = 3f;

    private IEventSubscription? _updateEventSubscription;
    private Vector2 _savedVelocity;
    private CancellationToken _cancellationToken;
    private CancellationTokenRegistration _ctr;

    public
        ProjectileSlower(IProjectile projectile, CancellationToken cancellationToken, IExtendedEvents extendedEvents) =>
        (_projectile, _savedVelocity, _cancellationToken, _extendedEvents, _trajectoryNodes) = (projectile,
            projectile.Velocity, cancellationToken, extendedEvents, new List<ProjectileTrajectoryNode>());

    public void Initialize()
    {
        var velocity = _projectile.Velocity;
        velocity.Normalize();
        _projectile.Velocity = velocity;

        var position = _projectile.Position;
        _trajectoryNodes.Add(new ProjectileTrajectoryNode(position));

        _updateEventSubscription = _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
        _ctr = _cancellationToken.Register(Stop);
    }

    public void Stop()
    {
        _updateEventSubscription?.Dispose();
        _projectile.Velocity = _savedVelocity;
        _ctr.Dispose();
    }

    private void OnUpdate(Event<float> @event)
    {
        var position = _projectile.Position;
        var previousNode = _trajectoryNodes.Last();
        var newPosition = position + ((1 / SlowMoModifier) * (position - previousNode.Position));

        _projectile.Position = position;

        var node = new ProjectileTrajectoryNode(newPosition);
        _trajectoryNodes.Add(node);
    }

    private readonly record struct ProjectileTrajectoryNode(Vector2 Position);
}