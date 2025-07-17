using System;
using LuckyBlocks.Extensions;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils.Watchers;

[Inject]
internal class PortalsWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    private static IObjectPortal[]? _portals;

    private readonly IPlayer _playerInstance;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Action<IObjectPortal>? _portalEnteredCallback;
    private readonly Action<IObjectPortal>? _portalExitedCallback;

    private IEventSubscription? _updateSubscription;
    private Vector2 _previousPlayerPosition;
    private int _awaitingExitPortalId;

    public PortalsWatcher(IPlayer playerInstance, IGame game, IExtendedEvents extendedEvents,
        Action<IObjectPortal>? portalEnteredCallback = null, Action<IObjectPortal>? portalExitedCallback = null)
    {
        _playerInstance = playerInstance;
        _game = game;
        _extendedEvents = extendedEvents;
        _portalEnteredCallback = portalEnteredCallback;
        _portalExitedCallback = portalExitedCallback;
    }

    public void Initialize()
    {
        _portals ??= _game.GetObjects<IObjectPortal>();
        _updateSubscription = _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void Dispose()
    {
        _updateSubscription?.Dispose();
    }

    private void OnUpdate(Event<float> @event)
    {
#if DEBUG
        if (_game.IsEditorTest)
        {
            _game.DrawArea(_playerInstance.GetAABB(), Color.Blue);

            foreach (var objectPortal in _portals!)
            {
                _game.DrawArea(objectPortal.GetAABB(), Color.Red);
            }
        }
#endif

        if (!_playerInstance.IsValid())
            return;

        var playerPosition = _playerInstance.GetWorldPosition();
        if (playerPosition == _previousPlayerPosition)
            return;

        _previousPlayerPosition = playerPosition;

        foreach (var objectPortal in _portals!)
        {
            if (_awaitingExitPortalId != 0)
            {
                if (objectPortal.UniqueId != _awaitingExitPortalId)
                    continue;

                var portalArea = objectPortal.GetAABB();
                var playerArea = _playerInstance.GetAABB();
                if (portalArea.Intersects(playerArea))
                {
                    _awaitingExitPortalId = 0;
                    _portalExitedCallback?.Invoke(objectPortal);

                    Logger.Debug("Player {Player} exit from portal {PortalId}", _playerInstance.Name,
                        objectPortal.UniqueId);
                }

                return;
            }

            if (objectPortal.HitTest(playerPosition))
            {
                var exitPortal = objectPortal.GetDestinationPortal();
                _awaitingExitPortalId = exitPortal.UniqueId;

                _portalEnteredCallback?.Invoke(objectPortal);

                Logger.Debug("Player entered portal {PortalId}", objectPortal.UniqueId);
                return;
            }
        }
    }
}