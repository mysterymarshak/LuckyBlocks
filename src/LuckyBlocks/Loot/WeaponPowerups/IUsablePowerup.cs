using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal interface IUsablePowerup<out T> : IWeaponPowerup<T> where T : Weapon
{
    int UsesCount { get; }
    int UsesLeft { get; }
    void ApplyAgain(IPlayer? player);
    void InvalidateWeapon(IPlayer player);
}