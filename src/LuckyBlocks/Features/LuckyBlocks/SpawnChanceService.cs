using System;
using System.Collections.Generic;
using LuckyBlocks.Features.Configuration;
using Serilog;

namespace LuckyBlocks.Features.LuckyBlocks;

internal interface ISpawnChanceService
{
    bool ChanceCanBeIncreased { get; }
    int ChanceId { get; }
    double Chance { get; }
    void Increase();
    void SetChance(int chanceId);
}

internal class SpawnChanceService : ISpawnChanceService
{
    public bool ChanceCanBeIncreased => !Configuration.IsManualSpawnChance && Chances.Count > ChanceId + 1;
    public int ChanceId { get; private set; }
    public double Chance => Configuration.IsManualSpawnChance ? Configuration.SpawnChance : Chances[ChanceId];

    private ISpawnChangeServiceConfiguration Configuration =>
        _configurationService.GetConfiguration<ISpawnChangeServiceConfiguration>();

    private static readonly IReadOnlyList<double> Chances = new List<double> { 0.3, 0.45, 0.6 };

    private readonly IConfigurationService _configurationService;
    private readonly ILogger _logger;

    public SpawnChanceService(IConfigurationService configurationService, ILogger logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public void Increase()
    {
        if (Configuration.IsManualSpawnChance || ChanceId >= Chances.Count)
        {
            throw new InvalidOperationException("can't increase chance");
        }

        ChanceId++;
    }

    public void SetChance(int chanceId)
    {
        if (Configuration.IsManualSpawnChance)
            return;

        if (ChanceId != chanceId)
        {
            ChanceId = chanceId;
            _logger.Information("LUCKY BLOCKS DROP CHANCE SET TO {Chance}%", Chance * 100);
        }
    }
}