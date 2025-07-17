using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal class TimeStoppedProjectile : ITimeStoppedEntity
{
    public Vector2 Position { get; private set; }

    private readonly IProjectile _projectile;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Vector2 _velocity;
    private readonly Vector2 _direction;

    private IEventSubscription? _updateEventSubscription;

    public TimeStoppedProjectile(IProjectile projectile, IExtendedEvents extendedEvents)
        => (_projectile, _velocity, _direction, _extendedEvents) =
            (projectile, projectile.Velocity, projectile.Direction, extendedEvents);

    public void Initialize()
    {
        Position = _projectile.Position;

        var velocity = _velocity;
        velocity.Normalize();
        _projectile.Velocity = velocity;

        _updateEventSubscription = _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void ResumeTime()
    {
        Dispose();

        _projectile.Velocity = _velocity;
        _projectile.Position = Position;
        _projectile.Direction = _direction;
    }

    private void OnUpdate(Event<float> @event)
    {
        _projectile.Position = Position;
        _projectile.Direction = _direction;
    }

    private void Dispose()
    {
        _updateEventSubscription?.Dispose();
    }
}