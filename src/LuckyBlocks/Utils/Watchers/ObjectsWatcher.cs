using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils.Watchers;

internal interface IObjectsWatcher
{
    IReadOnlyDictionary<int, IObject> DynamicObjects { get; }
    IReadOnlyDictionary<int, IObject> BackgroundObjects { get; }
    IReadOnlyDictionary<int, IObject> StaticIObjects { get; }
    IReadOnlyDictionary<int, IObject> StaticObjects { get; }
    IEnumerable<IObject> AllObjects { get; }
    void Initialize();
}

internal class ObjectsWatcher : IObjectsWatcher
{
    public IReadOnlyDictionary<int, IObject> DynamicObjects => _dynamicObjects!;
    public IReadOnlyDictionary<int, IObject> BackgroundObjects => _backgroundObjects!;
    public IReadOnlyDictionary<int, IObject> StaticObjects => _staticObjects!;
    public IReadOnlyDictionary<int, IObject> StaticIObjects => _staticIObjects!;

    public IEnumerable<IObject> AllObjects => BackgroundObjects.Values
        .Concat(DynamicObjects.Values)
        .Concat(StaticObjects.Values);

    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;

    private Dictionary<int, IObject>? _dynamicObjects;
    private Dictionary<int, IObject>? _backgroundObjects;
    private Dictionary<int, IObject>? _staticObjects;
    private Dictionary<int, IObject>? _staticIObjects;

    public ObjectsWatcher(IGame game, ILifetimeScope lifetimeScope)
    {
        _game = game;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        _dynamicObjects = _game
            .GetObjects<IObject>()
            .Where(IsDynamicObject)
            .ToDictionary(x => x.UniqueId, x => x);

        _backgroundObjects = _game
            .GetObjects<IObject>()
            .Where(x => x.Name.Contains("Bg"))
            .ToDictionary(x => x.UniqueId, x => x);

        _staticObjects = _game
            .GetObjects<IObject>()
            .Where(x => x.GetBodyType() == BodyType.Static)
            .ToDictionary(x => x.UniqueId, x => x);

        _staticIObjects = _staticObjects
            .Where(x => x.GetType().BaseType!.Name == nameof(IObject))
            .ToDictionary(x => x.Key, x => x.Value);

        _extendedEvents.HookOnCreated(OnObjectsCreated, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectsDestroyed, EventHookMode.Default);
    }

    private void OnObjectsCreated(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (IsDynamicObject(@object))
            {
                _dynamicObjects!.Add(@object.UniqueId, @object);
                continue;
            }

            if (@object.GetType().BaseType!.Name == nameof(IObject))
            {
                _staticIObjects!.Add(@object.UniqueId, @object);
            }
        }
    }

    private void OnObjectsDestroyed(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (_dynamicObjects!.Remove(@object.UniqueId) == false)
            {
                _staticObjects!.Remove(@object.UniqueId);
                _staticIObjects!.Remove(@object.UniqueId);
            }
        }
    }

    private static bool IsDynamicObject(IObject @object) => @object.GetCollisionFilter().MaskBits != 0 &&
                                                            (@object.GetBodyType() == BodyType.Dynamic ||
                                                             @object is
                                                                 IObjectSupplyCrate) && // supply crate right after spawn is static
                                                            @object.GetPhysicsLayer() == PhysicsLayer.Active;
}