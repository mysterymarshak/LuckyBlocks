using System;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Loot;
using LuckyBlocks.Repositories;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.LuckyBlocks;

internal interface ILuckyBlocksService
{
    void Initialize();
    void OnLuckyBlockBroken(LuckyBlockBrokenArgs args);
    void CreateLuckyBlock(IObjectSupplyCrate supplyCrate);
}

internal class LuckyBlocksService : ILuckyBlocksService
{
    private readonly IGame _game;
    private readonly ILuckyBlocksRepository _luckyBlocksRepository;
    private readonly ISpawnChanceService _spawnChanceService;
    private readonly IRandomItemProvider _randomItemProvider;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ILootFactory _lootFactory;
    private readonly ILogger _logger;

    public LuckyBlocksService(IGame game, ILuckyBlocksRepository luckyBlocksRepository,
        ISpawnChanceService spawnChanceService, IRandomItemProvider randomItemProvider, IEffectsPlayer effectsPlayer,
        ILootFactory lootFactory, ILogger logger) =>
        (_game, _luckyBlocksRepository, _spawnChanceService, _randomItemProvider, _effectsPlayer, _lootFactory,
            _logger) = (game, luckyBlocksRepository, spawnChanceService, randomItemProvider, effectsPlayer, lootFactory,
            logger);

    public void Initialize()
    {
        Events.ObjectCreatedCallback.Start(OnObjectsCreated);
        _logger.Information("Lucky blocks started ~ {Chance}%", _spawnChanceService.Chance * 100);
    }

    public void CreateLuckyBlock(IObjectSupplyCrate supplyCrate)
    {
        var luckyBlock = _luckyBlocksRepository.CreateLuckyBlock(supplyCrate);
        _logger.Debug("Lucky block with id '{LuckyBlockId}' created", luckyBlock.Id);
    }

    public void OnLuckyBlockBroken(LuckyBlockBrokenArgs args)
    {
        var item = _randomItemProvider.GetRandomItem(args);
        var createLootResult = _lootFactory.CreateLoot(args, item);

        if (createLootResult.TryPickT1(out var error, out var loot))
        {
            _logger.Error("Error in ILootFactory.Create: {Error}", error.Value);
            return;
        }

        loot.Run();
        
        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, args.Position, loot.Name);
        _logger.Information("Lucky block id '{LuckyBlockId}' loot: {LootName}", args.LuckyBlockId, loot.Name);

        _luckyBlocksRepository.RemoveLuckyBlock(args.LuckyBlockId);
    }

    private void OnObjectsCreated(IObject[] objects)
    {
        try
        {
            if (_game.IsGameOver)
                return;

            var supplyCrates = objects
                .Where(x => x is IObjectSupplyCrate)
                .Where(x => !_luckyBlocksRepository.IsLuckyBlockExists(x.UniqueId))
                .Cast<IObjectSupplyCrate>();

            foreach (var supplyCrate in supplyCrates)
            {
                if (!_spawnChanceService.Randomize())
                    continue;

                CreateLuckyBlock(supplyCrate);
            }
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in LuckyBlocksService.OnObjectsCreated");
        }
    }
}