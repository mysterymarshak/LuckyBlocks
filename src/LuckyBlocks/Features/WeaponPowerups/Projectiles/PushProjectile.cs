using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class PushProjectile : ProjectilePowerupBase
{
    protected override float ProjectileSpeedModifier => 1 / 4.5f;

    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    private IEventSubscription? _updateEventSubscription;

    public PushProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args) : base(
        projectile, extendedEvents, args)
    {
        _game = args.Game;
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new PushProjectile(Projectile, ExtendedEvents, _args);
    }

    protected override void OnRunInternal()
    {
        _updateEventSubscription = ExtendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    protected override void OnDisposedInternal()
    {
        _updateEventSubscription?.Dispose();
    }

    private void OnUpdate(Event<float> @event)
    {
        const int diagonalHalf = 10;

        var position = Projectile.Position;
        var min = new Vector2(position.X - diagonalHalf, position.Y - diagonalHalf);
        var max = new Vector2(position.X + diagonalHalf, position.Y + diagonalHalf);
        var area = new Area(min, max);

        var objects = _game
            .GetObjectsByArea(area)
            .Where(x => x.GetBodyType() != BodyType.Static);

        foreach (var @object in objects)
        {
            if (@object.UniqueId == Projectile.InitialOwnerPlayerID)
                continue;

            if (@object is IPlayer playerInstance)
            {
                playerInstance.LiftUp();
            }

            @object.SetLinearVelocity(Projectile.Velocity / 20);
        }
    }
}