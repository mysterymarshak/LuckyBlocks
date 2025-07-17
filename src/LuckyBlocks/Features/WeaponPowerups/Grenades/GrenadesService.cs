using System.Collections.Generic;
using System.Linq;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Grenades;

internal interface IGrenadesService
{
    void AddPowerup(IObjectGrenadeThrown grenadeThrown, GrenadeBase powerup);
    List<GrenadeBase> GetPowerupsCopy(IObjectGrenadeThrown grenadeThrown);
    void ApplyPowerups(IObjectGrenadeThrown grenadeThrown, IEnumerable<GrenadeBase> powerups);
}

internal class GrenadesService : IGrenadesService
{
    private readonly Dictionary<IObjectGrenadeThrown, List<GrenadeBase>> _grenades = new();

    public void AddPowerup(IObjectGrenadeThrown grenadeThrown, GrenadeBase powerup)
    {
        if (!_grenades.TryGetValue(grenadeThrown, out var powerups))
        {
            powerups = [];
            _grenades.Add(grenadeThrown, powerups);
        }

        powerup.Destroy += OnGrenadeDestroyed;
        powerups.Add(powerup);

        if (powerup.IsCloned)
        {
            powerup.MoveTo(grenadeThrown);
        }
        else
        {
            powerup.Initialize();
        }
    }

    public List<GrenadeBase> GetPowerupsCopy(IObjectGrenadeThrown grenadeThrown)
    {
        if (!_grenades.TryGetValue(grenadeThrown, out var powerups))
        {
            return [];
        }

        return powerups
            .Select(x => x.Clone())
            .ToList();
    }

    public void ApplyPowerups(IObjectGrenadeThrown grenadeThrown, IEnumerable<GrenadeBase> powerups)
    {
        foreach (var powerup in powerups)
        {
            AddPowerup(grenadeThrown, powerup);
        }
    }

    private void OnGrenadeDestroyed(IObjectGrenadeThrown grenadeThrown, GrenadeBase powerup)
    {
        powerup.Destroy -= OnGrenadeDestroyed;

        var powerups = _grenades[grenadeThrown];
        powerups.Remove(powerup);

        if (powerups.Count == 0)
        {
            _grenades.Remove(grenadeThrown);
        }
    }
}