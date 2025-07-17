using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedSupplyCrate : RevertedDynamicObject
{
    private readonly WeaponItem _weaponItem;
    private readonly SupplyCategoryType _supplyCategoryType;
    private readonly bool _isLuckyBlock;

    public RevertedSupplyCrate(IObjectSupplyCrate supplyCrate) : base(supplyCrate)
    {
        _weaponItem = supplyCrate.GetWeaponItem();
        _supplyCategoryType = supplyCrate.GetSupplyCategoryType();
        _isLuckyBlock = supplyCrate.CustomId == "LuckyBlock";
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        var supplyCrate = (IObjectSupplyCrate)Object;

        if (!_isLuckyBlock)
        {
            if (supplyCrate.GetWeaponItem() != _weaponItem)
            {
                supplyCrate.SetWeaponItem(_weaponItem);
            }

            if (supplyCrate.GetSupplyCategoryType() != _supplyCategoryType)
            {
                supplyCrate.SetSupplyCategoryType(_supplyCategoryType);
            }
        }
    }

    protected override IObject? Respawn(IGame game)
    {
        var supplyCrate = game.SpawnSupplyCrate();

        supplyCrate.SetWorldPosition(WorldPosition);
        supplyCrate.SetLinearVelocity(LinearVelocity);
        supplyCrate.SetAngularVelocity(AngularVelocity);
        supplyCrate.SetAngle(Angle);
        supplyCrate.SetFaceDirection(Direction);

        if (!_isLuckyBlock)
        {
            supplyCrate.CustomId = "CannotBeLuckyBlock";
        }

        return supplyCrate;
    }
}