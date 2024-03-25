using Autofac;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.ShockedObjects;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Loot;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record BuffConstructorArgs(INotificationService NotificationService,
    IPlayerModifiersService PlayerModifiersService, IEffectsPlayer EffectsPlayer, IBuffsService BuffsService,
    IMediator Mediator, IMagicService MagicService, IMagicFactory MagicFactory,
    IShockedObjectsService ShockedObjectsService, IIdentityService IdentityService, IDialoguesService DialoguesService,
    IObjectsWatcher ObjectsWatcher, ITimeStopService TimeStopService, IGame Game, ILogger Logger, ILifetimeScope LifetimeScope);