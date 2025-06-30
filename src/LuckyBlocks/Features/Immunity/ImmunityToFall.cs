using Autofac;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Immunity;

internal class ImmunityToFall : IApplicableImmunity
{
    public string Name => "Immunity to fall";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToFall;

    private readonly IPlayer _player;
    private readonly IExtendedEvents _extendedEvents;
    private readonly ILifetimeScope _lifetimeScope;

    private float _previousHealth;
    
    public ImmunityToFall(IPlayer player, ILifetimeScope lifetimeScope)
        => (_player, _lifetimeScope, _extendedEvents) =
            (player, lifetimeScope, lifetimeScope.Resolve<IExtendedEvents>());

    public void Apply()
    {
        _extendedEvents.HookOnDamage(_player, OnDamaged, EventHookMode.Default);
        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void Remove()
    {
        _lifetimeScope.Dispose();
        _extendedEvents.Clear();
    }

    private void OnUpdate(Event<float> @event)
    {
        var health = _player.GetHealth();
        if (health == 0)
            return;
        
        _previousHealth = health;
    }

    private void OnDamaged(Event<PlayerDamageArgs> @event)
    {
        var args = @event.Args;

        if (args.DamageType != PlayerDamageEventType.Fall)
            return;

        var health = _player.GetHealth();
        var overkillDamage = args.Damage >= health;
        // flag args.OverkillDamage indicates if player is already dead and takes damage

        if (overkillDamage)
        {
            _player.SetHealth(_previousHealth);
            return;
        }

        _player.SetHealth(_player.GetHealth() + args.Damage);
    }
}