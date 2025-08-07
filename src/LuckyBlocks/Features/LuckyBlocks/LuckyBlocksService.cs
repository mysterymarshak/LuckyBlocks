using System;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Entities;
using LuckyBlocks.Loot;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.LuckyBlocks;

internal interface ILuckyBlocksService
{
    void Initialize();
    void CreateLuckyBlock(IObjectSupplyCrate supplyCrate, Item predefinedItem = Item.None);
    void OnLuckyBlockBroken(LuckyBlockBrokenArgs args);
}

internal class LuckyBlocksService : ILuckyBlocksService
{
    private readonly IGame _game;
    private readonly ISpawnChanceService _spawnChanceService;
    private readonly IRandomItemProvider _randomItemProvider;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ILootFactory _lootFactory;
    private readonly IEntitiesService _entitiesService;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IExtendedEvents _extendedEvents;

    public LuckyBlocksService(IGame game, ISpawnChanceService spawnChanceService,
        IRandomItemProvider randomItemProvider, IEffectsPlayer effectsPlayer, ILootFactory lootFactory,
        IEntitiesService entitiesService, ILifetimeScope lifetimeScope, IMediator mediator, ILogger logger)
    {
        _game = game;
        _spawnChanceService = spawnChanceService;
        _randomItemProvider = randomItemProvider;
        _effectsPlayer = effectsPlayer;
        _lootFactory = lootFactory;
        _entitiesService = entitiesService;
        _mediator = mediator;
        _logger = logger;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        Events.ObjectCreatedCallback.Start(OnObjectsCreated);

#if PUBLICRELEASE
        _logger.Information("Lucky blocks started ~ {Chance}% (/lb_help)", _spawnChanceService.Chance * 100);
#else
        _logger.Information("Lucky blocks DEV started ~ {Chance}% (/lb_help)", _spawnChanceService.Chance * 100);
#endif
    }

    public void CreateLuckyBlock(IObjectSupplyCrate supplyCrate, Item predefinedItem = Item.None)
    {
        var luckyBlock = new LuckyBlock(supplyCrate, _mediator, _extendedEvents, predefinedItem);
        luckyBlock.Initialize();
        _entitiesService.Add(luckyBlock);

        _logger.Debug("Lucky block with id '{ObjectId}' created", supplyCrate.UniqueId);
    }

    public void OnLuckyBlockBroken(LuckyBlockBrokenArgs args)
    {
        if (args.ShouldHandle)
        {
            var item = args.PredefinedItem == Item.None ? _randomItemProvider.GetRandomItem(args) : args.PredefinedItem;

            if (item == Item.None)
            {
                _logger.Error("No lucky block loot for applying configuration and current circumstances found");
                _entitiesService.Remove(args.LuckyBlockId);

                _game.SpawnWeaponItem(_game.GetRandomWeaponItem(), args.Position, true, 20_000f);

                return;
            }

            var createLootResult = _lootFactory.CreateLoot(args, item);
            if (createLootResult.TryPickT1(out var error, out var loot))
            {
                _logger.Error("Error in ILootFactory.Create: {Error}", error.Value);
                return;
            }

            loot.Run();

            _effectsPlayer.PlayEffect(EffectName.CustomFloatText, args.Position, loot.Name);

            if (args.IsPlayer)
            {
                _logger.Information("{LootName} for {PlayerName}", loot.Name, _game.GetPlayer(args.PlayerId).Name);
            }
            else
            {
                _logger.Information("{LootName}", loot.Name);
            }
        }

        _entitiesService.Remove(args.LuckyBlockId);
    }

    private void OnObjectsCreated(IObject[] objects)
    {
        try
        {
            if (_game.IsGameOver)
                return;

            var supplyCrates = objects
                .Where(x => x is IObjectSupplyCrate { CustomId: not "CannotBeLuckyBlock" })
                .Where(x => !_entitiesService.IsRegistered(x))
                .Cast<IObjectSupplyCrate>();

            Awaiter.Start(delegate
            {
                foreach (var supplyCrate in supplyCrates)
                {
                    if (SharedRandom.Instance.NextDouble() > _spawnChanceService.Chance &&
                        supplyCrate.CustomId != "ShouldBeLuckyBlock")
                        continue;

                    CreateLuckyBlock(supplyCrate);
                }
            }, TimeSpan.Zero);
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in LuckyBlocksService.OnObjectsCreated");
        }
    }
}