using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.WeaponPowerups.Grenades;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Args;

internal record PowerupConstructorArgs(
    IBuffsService BuffsService,
    IEffectsPlayer EffectsPlayer,
    INotificationService NotificationService,
    IIdentityService IdentityService,
    IProjectilesService ProjectilesService,
    IGrenadesService GrenadesService,
    IMediator Mediator,
    IGame Game,
    BuffConstructorArgs BuffConstructorArgs,
    ILifetimeScope LifetimeScope);