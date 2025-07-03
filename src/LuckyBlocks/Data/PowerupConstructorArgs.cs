using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record PowerupConstructorArgs(
    IWeaponPowerupsService WeaponsPowerupsService,
    IBuffsService BuffsService,
    IEffectsPlayer EffectsPlayer,
    INotificationService NotificationService,
    IIdentityService IdentityService,
    IMediator Mediator,
    IGame Game,
    ILogger Logger,
    BuffConstructorArgs BuffConstructorArgs,
    ILifetimeScope LifetimeScope);