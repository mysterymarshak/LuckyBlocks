using Autofac;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Immunity;

internal class ImmunityToFall : IApplicableImmunity
{
    public string Name => "Immunity to fall";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToFall;

    private readonly IPlayer _player;
    private readonly IExtendedEvents _extendedEvents;
    private readonly ILifetimeScope _lifetimeScope;

    public ImmunityToFall(IPlayer player, ILifetimeScope lifetimeScope)
        => (_player, _lifetimeScope, _extendedEvents) = (player, lifetimeScope, lifetimeScope.Resolve<IExtendedEvents>());
    
    public void Apply()
    {
        _extendedEvents.HookOnDamage(_player, OnDamaged, EventHookMode.Default);
    }

    public void Remove()
    {
        _lifetimeScope.Dispose();
        _extendedEvents.Clear();
    }

    private void OnDamaged(Event<PlayerDamageArgs> @event)
    {
        var args = @event.Args;
        
        if (args.DamageType != PlayerDamageEventType.Fall)
            return;
        
        if (args.OverkillDamage)
            return;

        _player.SetHealth(_player.GetHealth() + args.Damage);
    }
}