using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.Time.TimeRevert;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Args;

internal record BuffConstructorArgs(
    INotificationService NotificationService,
    IPlayerModifiersService PlayerModifiersService,
    IEffectsPlayer EffectsPlayer,
    IBuffsService BuffsService,
    IMagicService MagicService,
    IMagicFactory MagicFactory,
    IIdentityService IdentityService,
    ITimeStopService TimeStopService,
    ITimeRevertService TimeRevertService,
    IGame Game,
    ILogger Logger,
    ILifetimeScope LifetimeScope);