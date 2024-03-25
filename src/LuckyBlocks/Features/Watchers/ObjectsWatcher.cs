using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IObjectsWatcher
{
    IReadOnlyList<IObject> DynamicObjects { get; }
    IReadOnlyList<IObject> BackgroundObjects { get; }
    IEnumerable<IObject> AllObjects { get; }
    void Initialize();
}

internal class ObjectsWatcher : IObjectsWatcher
{
    public IReadOnlyList<IObject> DynamicObjects => _dynamicObjects!;
    public IReadOnlyList<IObject> BackgroundObjects => _backgroundObjects!;
    public IReadOnlyList<IObject> StaticObjects => _staticObjects!;
    public IEnumerable<IObject> AllObjects =>  BackgroundObjects.Concat(DynamicObjects).Concat(StaticObjects);
    
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    
    private List<IObject>? _dynamicObjects;
    private List<IObject>? _backgroundObjects;
    private List<IObject>? _staticObjects;

    public ObjectsWatcher(IGame game, ILifetimeScope lifetimeScope)
        => (_game, _extendedEvents) = (game, lifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>());
    
    public void Initialize()
    {
        _dynamicObjects = _game
            .GetObjects<IObject>()
            .Where(x => x.GetBodyType() == BodyType.Dynamic)
            .Where(x => x.GetPhysicsLayer() == PhysicsLayer.Active)
            .Where(x => x.GetCollisionFilter().MaskBits != 0)
            .ToList();
        
        _backgroundObjects = _game
            .GetObjects<IObject>()
            .Where(x => x.Name.Contains("Bg"))
            .ToList();
        
        _staticObjects = _game
            .GetObjects<IObject>()
            .Where(x => x.GetBodyType() == BodyType.Static)
            .ToList();
            
        _extendedEvents.HookOnCreated(OnObjectsCreated, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectsDestroyed, EventHookMode.Default);
    }

    private void OnObjectsCreated(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (@object.GetCollisionFilter().MaskBits == 0)
                continue;
            
            _dynamicObjects!.Add(@object);
        }
    }

    private void OnObjectsDestroyed(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            _dynamicObjects!.Remove(@object);
        }
    }
}