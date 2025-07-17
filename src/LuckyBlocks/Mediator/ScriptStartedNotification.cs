using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Util;
using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Features.Commands;
using LuckyBlocks.Features.Elevators;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Features.ShockedObjects;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Time.TimeRevert;
using LuckyBlocks.Reflection;
using LuckyBlocks.Utils.Watchers;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Mediator;

internal readonly record struct ScriptStartedNotification : INotification;

internal class ScriptStartedNotificationHandler : INotificationHandler<ScriptStartedNotification>
{
    private readonly ILuckyBlocksService _luckyBlocksService;
    private readonly ICommandsHandler _commandsHandler;
    private readonly IPlayerDeathsWatcher _playerDeathsWatcher;
    private readonly IIdentityService _identityService;
    private readonly IGame _game;
    private readonly IWeaponsMapper _weaponsMapper;
    private readonly IWeaponsDataWatcher _weaponsDataWatcher;
    private readonly ITimeProvider _timeProvider;
    private readonly IObjectsWatcher _objectsWatcher;
    private readonly IShockedObjectsService _shockedObjectsService;
    private readonly ITimeRevertService _timeRevertService;
    private readonly ISnapshotCreator _snapshotCreator;
    private readonly IElevatorsService _elevatorsService;
    private readonly IMappedObjectsService _mappedObjectsService;
    private readonly ILogger _logger;

    public ScriptStartedNotificationHandler(ILuckyBlocksService luckyBlocksService, ICommandsHandler commandsHandler,
        IPlayerDeathsWatcher playerDeathsWatcher, IIdentityService identityService, IGame game,
        IWeaponsMapper weaponsMapper, IWeaponsDataWatcher weaponsDataWatcher, ITimeProvider timeProvider,
        IObjectsWatcher objectsWatcher, IShockedObjectsService shockedObjectsService,
        ITimeRevertService timeRevertService, ISnapshotCreator snapshotCreator, IElevatorsService elevatorsService,
        IMappedObjectsService mappedObjectsService, ILogger logger)
    {
        _luckyBlocksService = luckyBlocksService;
        _commandsHandler = commandsHandler;
        _playerDeathsWatcher = playerDeathsWatcher;
        _identityService = identityService;
        _game = game;
        _weaponsMapper = weaponsMapper;
        _weaponsDataWatcher = weaponsDataWatcher;
        _timeProvider = timeProvider;
        _objectsWatcher = objectsWatcher;
        _shockedObjectsService = shockedObjectsService;
        _timeRevertService = timeRevertService;
        _snapshotCreator = snapshotCreator;
        _elevatorsService = elevatorsService;
        _mappedObjectsService = mappedObjectsService;
        _logger = logger;
    }

    public ValueTask Handle(ScriptStartedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            InitializeStaticProperties();
        }
        catch (Exception exception)
        {
            _logger.Error(exception,
                "Unexpected exception while initializing static properties in ScriptStartedNotificationHandler.Handle");
        }

        try
        {
            InitializeServices();
        }
        catch (Exception exception)
        {
            _logger.Error(exception,
                "Unexpected exception while initializing services in ScriptStartedNotificationHandler.Handle");
        }

        return new ValueTask();
    }

    private void InitializeServices()
    {
        _identityService.Initialize();
        _luckyBlocksService.Initialize();
        _playerDeathsWatcher.Initialize();
        _commandsHandler.Initialize();
        _timeProvider.Initialize();
        _objectsWatcher.Initialize();
        _weaponsDataWatcher.Initialize();
        _shockedObjectsService.Initialize();
        _timeRevertService.Initialize();
        _snapshotCreator.Initialize();
        _elevatorsService.Initialize();
    }

    private void InitializeStaticProperties()
    {
        var assembly = typeof(AssemblyMarker).Assembly;
        var injectableTypes = assembly
            .GetLoadableTypes()
            .Where(x => x.IsClass)
            .Where(x => x.GetCustomAttribute<InjectAttribute>() is not null);
        var injectableProperties = injectableTypes
            .SelectMany(x => x.GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            .Where(x => x.GetCustomAttributes(typeof(InjectAttribute)).Any());

        foreach (var injectableProperty in injectableProperties)
        {
            var attribute = injectableProperty.GetCustomAttribute<InjectAttribute>();
            switch (attribute)
            {
                case InjectGameAttribute:
                    injectableProperty.SetValue(null, _game);
                    break;
                case InjectLoggerAttribute:
                    injectableProperty.SetValue(null, _logger);
                    break;
                case InjectWeaponsMapperAttribute:
                    injectableProperty.SetValue(null, _weaponsMapper);
                    break;
                case InjectTimeProviderAttribute:
                    injectableProperty.SetValue(null, _timeProvider);
                    break;
                case InjectMappedObjectsServiceAttribute:
                    injectableProperty.SetValue(null, _mappedObjectsService);
                    break;
            }
        }
    }
}