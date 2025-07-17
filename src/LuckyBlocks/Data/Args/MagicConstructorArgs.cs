using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.ShockedObjects;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Time.TimeRevert;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Args;

internal record MagicConstructorArgs(
    INotificationService NotificationService,
    IPlayerModifiersService PlayerModifiersService,
    IEffectsPlayer EffectsPlayer,
    IBuffsService BuffsService,
    IShockedObjectsService ShockedObjectsService,
    IIdentityService IdentityService,
    IDialoguesService DialoguesService,
    ITimeStopService TimeStopService,
    IWeaponPowerupsService WeaponPowerupsService,
    ITimeRevertService TimeRevertService,
    ITimeProvider TimeProvider,
    IGame Game,
    ILogger Logger,
    ILifetimeScope LifetimeScope);