using System.Collections.Generic;
using LuckyBlocks.Extensions;
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
        nameof(ISpawnChangeServiceConfiguration.SpawnChance)
    ];

    private readonly IGame _game;

    public ConfigurationService(IGame game)
    {
        _game = game;
    }

    public T GetConfiguration<T>() where T : class, IConfiguration
    {
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
            }
        }

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
                return;
            }
            case float floatValue:
            {
                _game.LocalStorage.SetItem(name, floatValue);
                return;
            }
        }
    }
}