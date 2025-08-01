using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Configuration;

internal interface IConfigurationService
{
    T GetConfiguration<T>() where T : class, IConfiguration;
    void UpdateProperty<T>(string name, T newValue);
}

internal class ConfigurationService : IConfigurationService
{
    private static readonly List<string> StorageKeys =
    [
        nameof(ISpawnChangeServiceConfiguration.IsManualSpawnChance),
        nameof(ISpawnChangeServiceConfiguration.SpawnChance),
        nameof(IRandomItemProviderConfiguration.ExcludedItems)
    ];

    private readonly IGame _game;

    private FullConfiguration? _configuration;
    private bool _onlyOnceFlag;

    public ConfigurationService(IGame game)
    {
        _game = game;
    }

    public T GetConfiguration<T>() where T : class, IConfiguration
    {
        if (_configuration is not null)
        {
            return (_configuration as T)!;
        }

        var configuration = new FullConfiguration(this);

        foreach (var key in StorageKeys)
        {
            switch (key)
            {
                case nameof(ISpawnChangeServiceConfiguration.IsManualSpawnChance):
                {
                    configuration.IsManualSpawnChance =
                        _game.LocalStorage.TryGetValueOrDefault(key, FullConfiguration.IsManualSpawnChanceDefault);
                    break;
                }
                case nameof(ISpawnChangeServiceConfiguration.SpawnChance):
                {
                    configuration.SpawnChance =
                        _game.LocalStorage.TryGetValueOrDefault(key, FullConfiguration.SpawnChanceDefault);
                    break;
                }
                case nameof(IRandomItemProviderConfiguration.ExcludedItems):
                {
                    configuration.ExcludedItems =
                        _game.LocalStorage.TryGetValueOrDefault(key, FullConfiguration.ExcludedItemsDefault);

                    if (!_onlyOnceFlag)
                    {
                        InvalidateExcludedItems(configuration);
                    }

                    break;
                }
            }
        }

        _onlyOnceFlag = true;
        configuration.SetInitialized();
        return (configuration as T)!;
    }

    public void UpdateProperty<T>(string name, T newValue)
    {
        switch (newValue)
        {
            case bool boolValue:
            {
                _game.LocalStorage.SetItem(name, boolValue);
                break;
            }
            case float floatValue:
            {
                _game.LocalStorage.SetItem(name, floatValue);
                break;
            }
            case IEnumerable<string> stringCollection:
            {
                _game.LocalStorage.SetItem(name, stringCollection.ToArray());
                break;
            }
        }

        InvalidateConfiguration();
    }

    private void InvalidateConfiguration()
    {
        _configuration = null;
    }

    private void InvalidateExcludedItems(IRandomItemProviderConfiguration configuration)
    {
        List<string>? newItems = null;

        foreach (var item in configuration.ExcludedItems)
        {
            if (!ItemExtensions.IsDefined(item))
            {
                newItems ??= configuration.ExcludedItems.ToList();
                newItems.Remove(item);
            }
        }

        if (newItems is not null)
        {
            configuration.ExcludedItems = newItems;
        }
    }
}