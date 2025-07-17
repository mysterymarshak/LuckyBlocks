using System;
using System.Collections.Generic;
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
    public bool ChanceCanBeIncreased => Chances.Count > ChanceId + 1;
    public int ChanceId { get; private set; }
    public double Chance => Chances[ChanceId];

    private static readonly IReadOnlyList<double> Chances = new List<double> { 0.3, 0.45, 0.6 };

    private readonly ILogger _logger;

    public SpawnChanceService(ILogger logger)
    {
        _logger = logger;
    }

    public void Increase()
    {
        if (ChanceId >= Chances.Count)
        {
            throw new InvalidOperationException("can't increase chance");
        }

        ChanceId++;
    }

    public void SetChance(int chanceId)
    {
        if (ChanceId != chanceId)
        {
            ChanceId = chanceId;
            _logger.Information("LUCKY BLOCKS DROP CHANCE SET TO {Chance}%", Chance * 100);
        }
    }
}