using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Util;
using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Features.Commands;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Reflection;
using LuckyBlocks.Wayback;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Notifications;

internal readonly record struct ScriptStartedNotification : INotification;

internal class ScriptStartedNotificationHandler : INotificationHandler<ScriptStartedNotification>
{
    private readonly ILuckyBlocksService _luckyBlocksService;
    private readonly ICommandsHandler _commandsHandler;
    private readonly IPlayerDeathsWatcher _playerDeathsWatcher;
    private readonly IWaybackMachine _waybackMachine;
    private readonly IGame _game;
    private readonly IWeaponsMapper _weaponsMapper;
    private readonly ITimeProvider _timeProvider;
    private readonly IObjectsWatcher _objectsWatcher;
    private readonly ILogger _logger;

    public ScriptStartedNotificationHandler(ILuckyBlocksService luckyBlocksService, ICommandsHandler commandsHandler,
        IPlayerDeathsWatcher playerDeathsWatcher, IWaybackMachine waybackMachine, IGame game,
        IWeaponsMapper weaponsMapper, ITimeProvider timeProvider, IObjectsWatcher objectsWatcher, ILogger logger) =>
        (_luckyBlocksService, _commandsHandler, _playerDeathsWatcher, _waybackMachine, _game, _weaponsMapper,
            _timeProvider, _objectsWatcher, _logger) = (luckyBlocksService, commandsHandler, playerDeathsWatcher,
            waybackMachine, game, weaponsMapper, timeProvider, objectsWatcher, logger);

    public ValueTask Handle(ScriptStartedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            InitializeServices();
            InitializeStaticProperties();
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in ScriptStartedNotificationHandler.Handle");
        }

        return new ValueTask();
    }

    private void InitializeServices()
    {
        _luckyBlocksService.Initialize();
        _playerDeathsWatcher.Initialize();
        _commandsHandler.Initialize();
        _timeProvider.Initialize();
        _objectsWatcher.Initialize();

        // _waybackMachine.Initialize();

        // legacy aim bullets
        // _playersTrajectoryWatcher.Start();
    }

    private void InitializeStaticProperties()
    {
        var assembly = typeof(ㅤ.ㅤ).Assembly;
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
            }
        }
    }
}