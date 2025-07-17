using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Watchers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Args;

internal record LootConstructorArgs(
    INotificationService NotificationService,
    IEffectsPlayer EffectsPlayer,
    IRespawner Respawner,
    IIdentityService IdentityService,
    IWeaponPowerupsService WeaponsPowerupsService,
    ISpawnChanceService SpawnChanceService,
    IBuffsService BuffsService,
    IPowerupFactory PowerupFactory,
    IAttributesChecker AttributesChecker,
    IWeaponsDataWatcher WeaponsDataWatcher,
    IGame Game,
    ILogger Logger,
    ILifetimeScope LifetimeScope);