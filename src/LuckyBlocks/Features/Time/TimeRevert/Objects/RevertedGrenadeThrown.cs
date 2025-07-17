using System.Collections.Generic;
using LuckyBlocks.Features.WeaponPowerups.Grenades;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedGrenadeThrown : RevertedDynamicObject
{
    private readonly IGrenadesService _grenadesService;
    private readonly float _dudChance;
    private readonly float _explosionTimer;
    private readonly List<GrenadeBase> _powerups;

    public RevertedGrenadeThrown(IObjectGrenadeThrown grenadeThrown, IGrenadesService grenadesService) : base(
        grenadeThrown)
    {
        _grenadesService = grenadesService;
        _dudChance = grenadeThrown.GetDudChance();
        _explosionTimer = grenadeThrown.GetExplosionTimer();
        _powerups = _grenadesService.GetPowerupsCopy(grenadeThrown);
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        var grenadeThrown = (IObjectGrenadeThrown)Object;

        if (!grenadeThrown.IsMissile)
        {
            grenadeThrown.TrackAsMissile(true);
        }

        if (grenadeThrown.GetDudChance() != _dudChance)
        {
            grenadeThrown.SetDudChance(_dudChance);
        }

        if (grenadeThrown.GetExplosionTimer() != _explosionTimer)
        {
            grenadeThrown.SetExplosionTimer(_explosionTimer);
        }

        _grenadesService.ApplyPowerups(grenadeThrown, _powerups);
    }
}