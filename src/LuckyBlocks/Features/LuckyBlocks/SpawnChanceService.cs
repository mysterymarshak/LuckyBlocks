using System;
using System.Collections.Generic;
using LuckyBlocks.Utils;

namespace LuckyBlocks.Features.LuckyBlocks;

internal interface ISpawnChanceService
{
    bool ChanceCanBeIncreased { get; }
    double Chance { get; }
    bool Randomize();
    void Increase();
}

internal class SpawnChanceService : ISpawnChanceService
{
    public bool ChanceCanBeIncreased => Chances.Count > _chanceId + 1;
    public double Chance => Chances[_chanceId];

    private static readonly IReadOnlyList<double> Chances = new List<double> { 0.3, 0.45, 0.6 };

    private int _chanceId;

    public bool Randomize()
    {
        return SharedRandom.Instance.NextDouble() <= Chance;
    }

    public void Increase()
    {
        if (_chanceId >= Chances.Count)
            throw new InvalidOperationException("can't increase chance");

        _chanceId++;
    }
}