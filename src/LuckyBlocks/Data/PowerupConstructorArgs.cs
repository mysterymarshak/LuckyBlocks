using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record PowerupConstructorArgs(IWeaponsPowerupsService WeaponsPowerupsService, IBuffsService BuffsService,
    IEffectsPlayer EffectsPlayer, INotificationService NotificationService,
    IPlayersTrajectoryWatcher PlayersTrajectoryWatcher, IIdentityService IdentityService, IMediator Mediator, IGame Game, ILogger Logger,
    BuffConstructorArgs BuffConstructorArgs, ILifetimeScope LifetimeScope);