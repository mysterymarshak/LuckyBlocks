using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.Bullets;
using LuckyBlocks.Features.WeaponPowerups.Melees;
using LuckyBlocks.Features.WeaponPowerups.ThrownItems;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class WeaponWithRandomPowerupsLoot : PowerUppedWeaponBase
{
    public override string Name => $"{_weaponItem} with [{string.Join(", ", _powerupsSet.Select(x => x.Name))}]";
    public override Item Item => Item.WeaponWithRandomPowerups;

    protected override WeaponItem WeaponItem => _weaponItem;

    private static readonly List<WeaponItem> Exceptions = [WeaponItem.FLAMETHROWER];
    private static readonly List<WeaponItem> Inclusions = [WeaponItem.GRENADES, WeaponItem.KATANA];

    private static readonly List<WeaponItem> WeaponItems = Enum.GetValues(typeof(WeaponItem))
        .Cast<WeaponItem>()
        .Where(x => x.GetWeaponItemType() is WeaponItemType.Handgun or WeaponItemType.Rifle)
        .Concat(Inclusions)
        .Except(Exceptions)
        .ToList();

    private static readonly Dictionary<Type, IEnumerable<Type>> FirearmPowerupsWithIncompatibility = new()
    {
        [typeof(AimBullets)] = AimBullets.IncompatiblePowerups,
        [typeof(ExplosiveBullets)] = ExplosiveBullets.IncompatiblePowerups,
        [typeof(FreezeBullets)] = FreezeBullets.IncompatiblePowerups,
        [typeof(InfiniteRicochetBullets)] = InfiniteRicochetBullets.IncompatiblePowerups,
        [typeof(PushBullets)] = PushBullets.IncompatiblePowerups,
        [typeof(TripleRicochetBullets)] = TripleRicochetBullets.IncompatiblePowerups,
        [typeof(PoisonBullets)] = PoisonBullets.IncompatiblePowerups
    };

    private static readonly Dictionary<Type, IEnumerable<Type>> GrenadePowerupsWithIncompatibility = new()
    {
        [typeof(StickyGrenades)] = StickyGrenades.IncompatiblePowerups,
        [typeof(BananaGrenades)] = BananaGrenades.IncompatiblePowerups
    };

    private static readonly Dictionary<int, List<List<Type>>> PossibleFirearmPowerupsCombinations;
    private static readonly int MaxPowerupsCount;

    private const double ChanceForIncreasingPowerupsCount = 0.3;

    private readonly WeaponItem _weaponItem;
    private readonly List<Type> _powerupsSet;
    private readonly IPowerupFactory _powerupFactory;

    static WeaponWithRandomPowerupsLoot()
    {
        var powerupCombinations = new Dictionary<int, List<List<Type>>>
        {
            [1] = FirearmPowerupsWithIncompatibility.Select(x => new List<Type> { x.Key }).ToList()
        };

        for (var i = 2; i <= FirearmPowerupsWithIncompatibility.Count; i++)
        {
            if (!powerupCombinations.TryGetValue(i - 1, out var powerupsSets))
                break;

            foreach (var powerupsSet in powerupsSets)
            {
                var additionalPowerup = powerupCombinations[1]
                    .SelectMany(x => x)
                    .Where(x => !powerupsSet.Contains(x))
                    .FirstOrDefault(x => powerupsSet.All(y => IsPowerupCompatible(x, y)));

                if (additionalPowerup is null)
                    continue;

                var newSet = powerupsSet.Append(additionalPowerup).ToList();
                var allSets = powerupCombinations.GetOrAdd(i, () => []);
                allSets.Add(newSet);
            }
        }

        PossibleFirearmPowerupsCombinations = powerupCombinations;
        MaxPowerupsCount = powerupCombinations.Keys.Max();
    }

    public WeaponWithRandomPowerupsLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _weaponItem = WeaponItems.GetRandomElement();
        _powerupFactory = args.PowerupFactory;
        _powerupsSet = GetRandomPowerups();
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        foreach (var powerupType in _powerupsSet)
        {
            yield return _powerupFactory.CreatePowerup(weapon, powerupType);
        }
    }

    private static bool IsPowerupCompatible(Type powerupType1, Type powerupType2)
    {
        if (FirearmPowerupsWithIncompatibility.TryGetValue(powerupType1, out var incompatibleFirearmPowerups))
        {
            return !incompatibleFirearmPowerups.Contains(powerupType2);
        }

        if (GrenadePowerupsWithIncompatibility.TryGetValue(powerupType1, out var incompatibleGrenadePowerups))
        {
            return !incompatibleGrenadePowerups.Contains(powerupType2);
        }

        throw new InvalidOperationException("invalid types passed");
    }

    private List<Type> GetRandomPowerups()
    {
        var powerupsCount = 1;
        while (powerupsCount < MaxPowerupsCount &&
               SharedRandom.Instance.NextDouble() < ChanceForIncreasingPowerupsCount)
        {
            powerupsCount++;
        }

        return _weaponItem switch
        {
            WeaponItem.GRENADES => [GrenadePowerupsWithIncompatibility.Keys.GetRandomElement()],
            WeaponItem.KATANA => [typeof(FlamyKatana)],
            _ => PossibleFirearmPowerupsCombinations[powerupsCount].GetRandomElement()
        };
    }
}