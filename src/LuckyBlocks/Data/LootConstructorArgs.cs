﻿using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record LootConstructorArgs(
    INotificationService NotificationService,
    IEffectsPlayer EffectsPlayer,
    IRespawner Respawner,
    IIdentityService IdentityService,
    IWeaponPowerupsService WeaponsPowerupsService,
    ISpawnChanceService SpawnChanceService,
    IBuffsService BuffsService,
    IMediator Mediator,
    IPowerupFactory PowerupFactory,
    IAttributesChecker AttributesChecker,
    IWeaponsDataWatcher WeaponsDataWatcher,
    IGame Game,
    ILogger Logger,
    ILifetimeScope LifetimeScope);