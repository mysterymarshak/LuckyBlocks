namespace LuckyBlocks.Features.Configuration;

internal interface ISpawnChangeServiceConfiguration : IConfiguration
{
    bool IsManualSpawnChance { get; set; }
    float SpawnChance { get; set; }
}