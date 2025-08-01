using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LuckyBlocks.Features.Configuration;

internal class FullConfiguration : ISpawnChangeServiceConfiguration, IRandomItemProviderConfiguration
{
    public static bool IsManualSpawnChanceDefault => false;
    public static float SpawnChanceDefault => 0.3f;
    public static List<string> ExcludedItemsDefault => [];

    public bool IsManualSpawnChance
    {
        get;
        set
        {
            field = value;

            if (!_isInitialized)
                return;

            Changes[nameof(IsManualSpawnChance)] =
                () => _configurationService.UpdateProperty(nameof(IsManualSpawnChance), value);
        }
    }

    public float SpawnChance
    {
        get;
        set
        {
            field = value;

            if (!_isInitialized)
                return;

            Changes[nameof(SpawnChance)] = () => _configurationService.UpdateProperty(nameof(SpawnChance), value);
        }
    }

    public IEnumerable<string> ExcludedItems
    {
        get;
        set
        {
            field = value;

            if (!_isInitialized)
                return;

            Changes[nameof(ExcludedItems)] = () => _configurationService.UpdateProperty(nameof(ExcludedItems), value);
        }
    }

    private readonly IConfigurationService _configurationService;

    [field: MaybeNull]
    private Dictionary<string, Action> Changes => field ??= new Dictionary<string, Action>();

    private bool _isInitialized;

    public FullConfiguration(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public void SetInitialized()
    {
        _isInitialized = true;
    }

    public void CommitChanges()
    {
        foreach (var change in Changes)
        {
            change.Value.Invoke();
        }
    }
}