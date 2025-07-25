using System;
using System.Linq;
using Autofac;
using Autofac.Util;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Entities;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Features.Time.TimeRevert;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.Bullets;
using LuckyBlocks.Features.WeaponPowerups.Melees;
using LuckyBlocks.Features.WeaponPowerups.ThrownItems;
using LuckyBlocks.Loot;
using LuckyBlocks.Loot.Events;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Watchers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Commands;

internal interface ICommandsHandler
{
    void Initialize();
}

internal class CommandsHandler : ICommandsHandler
{
    private readonly ILuckyBlocksService _luckyBlocksService;
    private readonly IIdentityService _identityService;
    private readonly IPowerupFactory _powerupFactory;
    private readonly IBuffFactory _buffFactory;
    private readonly IBuffsService _buffsService;
    private readonly IRespawner _respawner;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly LootConstructorArgs _lootArgs;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IWeaponsDataWatcher _weaponsDataWatcher;
    private readonly ITimeRevertService _timeRevertService;
    private readonly IMappedObjectsService _mappedObjectsService;
    private readonly IEntitiesService _entitiesService;
    private readonly ISnapshotCreator _snapshotCreator;
    private readonly ISpawnChanceService _spawnChanceService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IExtendedEvents _extendedEvents;

    public CommandsHandler(ILuckyBlocksService luckyBlocksService, IIdentityService identityService,
        IBuffFactory buffFactory, IPowerupFactory powerupFactory, IBuffsService buffsService, IRespawner respawner,
        LootConstructorArgs lootArgs, IGame game, ILogger logger, IWeaponPowerupsService weaponPowerupsService,
        ILifetimeScope lifetimeScope, IWeaponsDataWatcher weaponsDataWatcher, ITimeRevertService timeRevertService,
        IMappedObjectsService mappedObjectsService, IEntitiesService entitiesService, ISnapshotCreator snapshotCreator,
        ISpawnChanceService spawnChanceService, IEffectsPlayer effectsPlayer)
    {
        _luckyBlocksService = luckyBlocksService;
        _identityService = identityService;
        _powerupFactory = powerupFactory;
        _buffFactory = buffFactory;
        _buffsService = buffsService;
        _respawner = respawner;
        _game = game;
        _logger = logger;
        _lootArgs = lootArgs;
        _weaponPowerupsService = weaponPowerupsService;
        _weaponsDataWatcher = weaponsDataWatcher;
        _timeRevertService = timeRevertService;
        _mappedObjectsService = mappedObjectsService;
        _entitiesService = entitiesService;
        _snapshotCreator = snapshotCreator;
        _spawnChanceService = spawnChanceService;
        _effectsPlayer = effectsPlayer;
        _extendedEvents = lifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnMessage(@event => OnUserMessage(@event.Args), EventHookMode.Default);
    }

    private void OnUserMessage(UserMessageCallbackArgs args)
    {
        if (!args.IsCommand)
            return;

        var command = args.Command.ToLower();
        var commandArgs = args.CommandArguments;
        var user = args.User;
        var playerInstance = user.GetPlayer();
        var position = playerInstance?.GetWorldPosition() ?? Vector2.Zero;

        if (!command.StartsWith("lb_"))
            return;

        try
        {
            switch (command["lb_".Length..])
            {
                case "restart":
                {
#if DEBUG
                    _game.RunCommand("/stopscript luckyblocks_wtf");
                    _game.RunCommand("/startscript luckyblocks_wtf");
#else
                    _game.RunCommand("/stopscript LuckyBlocks");
                    _game.RunCommand("/startscript LuckyBlocks");
#endif
                    break;
                }
#if DEBUG
                case "spawn":
                {
                    var supplyCrate = (_game.CreateObject("SupplyCrate00", position) as IObjectSupplyCrate)!;

                    ItemExtensions.TryParse(commandArgs, out var predefinedItem, true);
                    _luckyBlocksService.CreateLuckyBlock(supplyCrate, predefinedItem);

                    break;
                }
                case "immunities":
                {
                    var player = _identityService.GetPlayerByInstance(playerInstance);

                    _logger.Debug("Immunities: {Immunities}", player.Immunities);

                    break;
                }
                case "buff":
                {
                    var splittedArgs = commandArgs.Split(' ');

                    var playerName = splittedArgs[0];
                    var buffName = splittedArgs[1];

                    var buffedPlayer = _game.GetActiveUsers().FirstOrDefault(x => x.Name == playerName)?.GetPlayer();
                    if (buffedPlayer is null)
                        return;

                    var buffType = typeof(AssemblyMarker).Assembly
                        .GetLoadableTypes()
                        .Where(x => x.GetInterfaces().Any(y => y.Name == nameof(IBuff)))
                        .FirstOrDefault(x => x.Name == buffName);

                    _logger.Debug("Type: {Type}", buffType);

                    if (buffType is null)
                        return;

                    var player = _identityService.GetPlayerByInstance(buffedPlayer);
                    var buff = _buffFactory.CreateBuff(player, buffType);
                    _buffsService.TryAddBuff(buff, player);

                    break;
                }
                case "powerups":
                {
                    var player = _identityService.GetPlayerByInstance(playerInstance);
                    var weaponsData = player.WeaponsData;
                    var drawn = weaponsData.CurrentWeaponDrawn;
                    var powerups = drawn.Powerups.Select(x => x.Name);

                    _logger.Debug("Powerups for {WeaponItem}: {Powerups}", drawn.WeaponItem,
                        string.Join(", ", powerups));

                    break;
                }
                case "kill":
                {
                    playerInstance?.Kill();

                    break;
                }
                case "respawn":
                {
                    var playerName = commandArgs;
                    var userToRespawn = _game.GetActiveUsers().FirstOrDefault(x => x.Name == playerName);

                    _respawner.RespawnPlayer(userToRespawn, userToRespawn.GetProfile());

                    break;
                }
                case "1hp":
                {
                    playerInstance?.SetHealth(1f);

                    break;
                }
                case "weapons":
                {
                    var player = string.IsNullOrWhiteSpace(commandArgs)
                        ? _identityService.GetPlayerByInstance(playerInstance)
                        : _identityService.GetPlayerByInstance(_game.GetActiveUsers()
                            .FirstOrDefault(x => x.Name == commandArgs).GetPlayer());
                    _logger.Debug("Data: {Data}", player.WeaponsData.ToString());
                    break;
                }
                case "powerup":
                {
                    var powerupType = typeof(AssemblyMarker).Assembly
                        .GetLoadableTypes()
                        .Where(x => x.GetInterfaces().Any(y => typeof(IWeaponPowerup<Weapon>).IsAssignableFrom(y)))
                        .FirstOrDefault(x => x.Name == commandArgs);

                    _logger.Debug("Type: {Type}", powerupType);

                    if (powerupType is null)
                        return;

                    var player = _identityService.GetPlayerByInstance(playerInstance);
                    var drawnWeapon = player.WeaponsData.CurrentWeaponDrawn;
                    var powerup = _powerupFactory.CreatePowerup(drawnWeapon, powerupType);
                    _weaponPowerupsService.AddWeaponPowerup(powerup, drawnWeapon);

                    break;
                }
                case "hp":
                {
                    playerInstance.SetHealth(100);
                    break;
                }
                case "lazer":
                {
                    var lazer = _game.CreateObject("BgPennant00B", position, MathHelper.PI / 2);
                    lazer.SetColor1("BgPink");
                    lazer.SetSizeFactor(new Point(5, 1));
                    break;
                }
                case "snapshot":
                {
                    _timeRevertService.TakeSnapshot();
                    break;
                }
                case "restore":
                {
                    _timeRevertService.RestoreFromSnapshot(_timeRevertService.Snapshots.Last().Id);
                    break;
                }
                case "chance":
                {
                    _spawnChanceService.Increase();
                    break;
                }
                case "effect":
                {
                    _effectsPlayer.PlayEffect(commandArgs, position);
                    break;
                }
                case "sound":
                {
                    _effectsPlayer.PlaySoundEffect(commandArgs, position);
                    break;
                }
#endif
            }
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in CommandsHandler.OnUserMessage");
        }
    }
}