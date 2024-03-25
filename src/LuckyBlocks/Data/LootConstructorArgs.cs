using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot;
using LuckyBlocks.Utils;
using LuckyBlocks.Wayback;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record LootConstructorArgs(INotificationService NotificationService, IEffectsPlayer EffectsPlayer,
    IRespawner Respawner, IIdentityService IdentityService, IWeaponsPowerupsService WeaponsPowerupsService,
    ISpawnChanceService SpawnChanceService, IBuffsService BuffsService, IMediator Mediator,
    IWaybackMachine WaybackMachine, IPowerupFactory PowerupFactory, IGame Game, ILogger Logger, ILifetimeScope LifetimeScope);