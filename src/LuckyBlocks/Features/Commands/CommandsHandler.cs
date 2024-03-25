﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Util;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot;
using LuckyBlocks.Loot.Buffs;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Wayback;
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
    private readonly IMagicFactory _magicFactory;
    private readonly IMagicService _magicService;
    private readonly BuffConstructorArgs _buffArgs;
    private readonly IIdentityService _identityService;
    private readonly IBuffFactory _buffFactory;
    private readonly IBuffsService _buffsService;
    private readonly IRespawner _respawner;
    private readonly IWaybackMachine _waybackMachine;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly PowerupConstructorArgs _powerupArgs;
    private readonly IWeaponsPowerupsService _weaponsPowerupsService;
    private readonly IExtendedEvents _extendedEvents;

    public CommandsHandler(ILuckyBlocksService luckyBlocksService, BuffConstructorArgs buffArgs,
        IIdentityService identityService, IBuffFactory buffFactory, IBuffsService buffsService, IRespawner respawner,
        IWaybackMachine waybackMachine, PowerupConstructorArgs powerupArgs, IGame game, ILogger logger,
        IWeaponsPowerupsService weaponsPowerupsService, ILifetimeScope lifetimeScope) =>
        (_luckyBlocksService, _magicFactory, _magicService, _buffArgs, _identityService, _buffFactory, _buffsService,
            _respawner, _waybackMachine, _powerupArgs, _game, _logger, _weaponsPowerupsService, _extendedEvents) = (
            luckyBlocksService, buffArgs.MagicFactory,
            buffArgs.MagicService, buffArgs, identityService, buffFactory, buffsService, respawner, waybackMachine,
            powerupArgs,
            game, logger, weaponsPowerupsService, lifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>());

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
                    _game.RunCommand("/stopscript LuckyBlocks");
                    _game.RunCommand("/startscript LuckyBlocks");

                    break;
                }
                case "spawn":
                {
                    var supplyCrate = (_game.CreateObject("SupplyCrate00", position) as IObjectSupplyCrate)!;

                    _luckyBlocksService.CreateLuckyBlock(supplyCrate);

                    break;
                }
                case "magic":
                {
                    var player = _identityService.GetPlayerByInstance(playerInstance);

                    var fireMagic = _magicFactory.CreateAreaMagic<FireMagic>(player, _buffArgs, 1);
                    var iceMagic = _magicFactory.CreateAreaMagic<ElectricMagic>(player, _buffArgs, -1);

                    _magicService.Cast(fireMagic, position - new Vector2(fireMagic.AreaSize.X * 1f, 0));
                    _magicService.Cast(iceMagic, position);

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
                    
                    var buffType = typeof(CommandsHandler).Assembly
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
                case "powerup":
                {
                    var weaponDrawn = playerInstance.CurrentWeaponDrawn;

                    if (weaponDrawn is not (WeaponItemType.Handgun or WeaponItemType.Rifle))
                        return;

                    var powerupType = typeof(CommandsHandler).Assembly
                        .GetLoadableTypes()
                        .Where(x => x.GetInterfaces().Any(y => y.Name == nameof(IFirearmPowerup)))
                        .FirstOrDefault(x => x.Name == commandArgs);

                    _logger.Debug("Type: {Type}", powerupType);

                    if (powerupType is null)
                        return;

                    var weaponsData = playerInstance.GetWeaponsData();
                    var firearm = (Firearm)weaponsData.GetWeaponByType(weaponDrawn);

                    var powerup = (IFirearmPowerup)Activator.CreateInstance(powerupType,
                        BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.OptionalParamBinding, null, new object[] { firearm, _powerupArgs },
                        CultureInfo.CurrentCulture);

                    _weaponsPowerupsService.AddWeaponPowerup(powerup, firearm, playerInstance);
                    
                    break;
                }
                case "knife":
                {
                    playerInstance.GiveWeaponItem(WeaponItem.KNIFE);
                    var weaponsWatcher = WeaponEventsWatcher.CreateForWeapon(WeaponItem.KNIFE,
                        WeaponItem.KNIFE.GetWeaponItemType(), playerInstance);

                    weaponsWatcher.Drop += (player, objectWeaponItem) =>
                    {
                        const float g = 9.8f / 9;
                        var a = new Vector2(0, -g);
                        
                        var initialVelocity = objectWeaponItem.GetLinearVelocity();
                        var startPosition = objectWeaponItem.GetWorldPosition();
                        
                        objectWeaponItem.SetAngularVelocity(0);
                        objectWeaponItem.SetAngle(0);

                        var previousPosition = startPosition;
                        var previousExpectedPosition = startPosition;
                        var startTime = _game.TotalElapsedGameTime;
                        
                        var pairs = new List<(Vector2, Vector2)>();
                        var pairsExpected = new List<(Vector2, Vector2)>();
                        
                        Events.UpdateCallback.Start(delegate
                        {
                            var deltaTime = (_game.TotalElapsedGameTime - startTime) / 1000;
                            deltaTime *= 20;
                            
                            var expectedPosition = startPosition + Displacement(initialVelocity, deltaTime, a);
                            
                            // expected
                            pairsExpected.ForEach(x => _game.DrawLine(x.Item1, x.Item2, Color.Red));
                            _game.DrawLine(previousExpectedPosition, expectedPosition, Color.Red);
                            
                            pairsExpected.Add(new (previousExpectedPosition, expectedPosition));
                            previousExpectedPosition = expectedPosition;
                            
                            // actual
                            pairs.ForEach(x => _game.DrawLine(x.Item1, x.Item2, Color.Yellow));
                            _game.DrawLine(previousPosition, objectWeaponItem.GetWorldPosition(), Color.Yellow);
                            
                            pairs.Add(new (previousPosition, objectWeaponItem.GetWorldPosition()));
                            previousPosition = objectWeaponItem.GetWorldPosition();
                        });

                        Vector2 Displacement(Vector2 v, float t, Vector2 a)
                        {
                            return v * t + a * t * t / 2;
                        }
                        
                        weaponsWatcher.Dispose();
                    };

                    weaponsWatcher.Start();
                    
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in CommandsHandler.OnUserMessage");
        }
    }
}