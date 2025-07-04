﻿using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Chat;
using LuckyBlocks.Features.Commands;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.ShockedObjects;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Logs;
using LuckyBlocks.Loot;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Loot.Buffs;
using LuckyBlocks.Repositories;
using LuckyBlocks.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks;

internal class LuckyBlocksModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var serviceCollection = new ServiceCollection().AddMediator();
        builder.Populate(serviceCollection);

        builder.Register<ILogger>(x =>
            new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.Chat(x.Resolve<IGame>(), x.Resolve<IChat>())
                .CreateLogger());

        builder.RegisterType<Chat>()
            .As<IChat>()
            .SingleInstance();

        builder.RegisterType<LuckyBlocksRepository>()
            .As<ILuckyBlocksRepository>()
            .SingleInstance();

        builder.RegisterType<SpawnChanceService>()
            .As<ISpawnChanceService>()
            .SingleInstance();

        builder.RegisterType<LuckyBlocksService>()
            .As<ILuckyBlocksService>()
            .SingleInstance();

        builder.RegisterType<PlayersRepository>()
            .As<IPlayersRepository>()
            .SingleInstance();

        builder.RegisterType<IdentityService>()
            .As<IIdentityService>()
            .SingleInstance();

        builder.RegisterType<AttributesChecker>()
            .As<IAttributesChecker>()
            .SingleInstance();

        builder.RegisterType<RandomItemProvider>()
            .As<IRandomItemProvider>()
            .SingleInstance();

        builder.RegisterType<LootFactory>()
            .As<ILootFactory>()
            .SingleInstance();

        builder.RegisterType<NotificationService>()
            .As<INotificationService>()
            .SingleInstance();

        builder.RegisterType<DialoguesService>()
            .As<IDialoguesService>()
            .SingleInstance();

        builder.RegisterType<BuffsService>()
            .As<IBuffsService>()
            .SingleInstance();

        builder.RegisterType<Respawner>()
            .As<IRespawner>()
            .SingleInstance();

        builder.RegisterType<EffectsPlayer>()
            .As<IEffectsPlayer>()
            .SingleInstance();

        builder.RegisterType<CommandsHandler>()
            .As<ICommandsHandler>()
            .SingleInstance();

        builder.RegisterType<WeaponPowerupsService>()
            .As<IWeaponPowerupsService>()
            .SingleInstance();
        
        builder.RegisterType<PlayerModifiersService>()
            .As<IPlayerModifiersService>()
            .SingleInstance();

        builder.RegisterType<BuffConstructorArgs>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<PowerupConstructorArgs>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<LootConstructorArgs>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<MagicService>()
            .As<IMagicService>()
            .SingleInstance();

        builder.RegisterType<MagicFactory>()
            .As<IMagicFactory>()
            .SingleInstance();

        builder.RegisterType<AttributesPriorityComparer>()
            .As<IComparer<ItemAttribute>>()
            .SingleInstance();

        builder.RegisterType<WeaponsMapper>()
            .As<IWeaponsMapper>()
            .SingleInstance();

        builder.RegisterType<BuffFactory>()
            .As<IBuffFactory>()
            .SingleInstance();

        builder.RegisterType<BuffWrapper>()
            .As<IBuffWrapper>()
            .SingleInstance();

        builder.RegisterType<ImmunityConstructorArgs>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<ImmunityService>()
            .As<IImmunityService>()
            .SingleInstance();

        builder.RegisterType<ExtendedEvents>()
            .As<IExtendedEvents>()
            .InstancePerLifetimeScope();

        builder.RegisterType<PlayerDeathsWatcher>()
            .As<IPlayerDeathsWatcher>()
            .SingleInstance();

        builder.RegisterType<ShockedObjectsRepository>()
            .As<IShockedObjectsRepository>()
            .SingleInstance();

        builder.RegisterType<ShockedObjectsService>()
            .As<IShockedObjectsService>()
            .SingleInstance();
        
        builder.RegisterType<TimeStopService>()
            .As<ITimeStopService>()
            .SingleInstance();

        builder.RegisterType<TimeProvider>()
            .As<ITimeProvider>()
            .SingleInstance();

        builder.RegisterType<ObjectsWatcher>()
            .As<IObjectsWatcher>()
            .SingleInstance();

        builder.RegisterType<PowerupFactory>()
            .As<IPowerupFactory>()
            .SingleInstance();

        builder.RegisterType<TimeStopper>()
            .As<ITimeStopper>()
            .SingleInstance();

        builder.RegisterType<WeaponsDataWatcher>()
            .As<IWeaponsDataWatcher>()
            .SingleInstance();
        
        builder.RegisterType<ThrowableWeaponsWatcher>()
            .As<IThrowableWeaponsWatcher>()
            .SingleInstance();
        
        builder.RegisterType<ReloadWeaponsWatcher>()
            .As<IReloadWeaponsWatcher>()
            .SingleInstance();

        builder.RegisterType<DrawnWeaponsWatcher>()
            .As<IDrawnWeaponsWatcher>()
            .SingleInstance();
        
        builder.RegisterType<ChainsawWeaponsWatcher>()
            .As<IChainsawWeaponsWatcher>()
            .SingleInstance();
    }
}