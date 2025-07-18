using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs.Durable;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Profiles;

internal interface IProfilesRepository
{
    IReadOnlyDictionary<string, IProfileClothingItem> GetChangedProfile<T>(IProfile profile) where T : IDurableBuff;
}

internal class ProfilesRepository : IProfilesRepository
{
    private static readonly Dictionary<Type, Func<IProfile, IReadOnlyDictionary<string, IProfileClothingItem>>>
        Profiles = new()
        {
            [typeof(Freeze)] = profile => GetSingleColorProfile(profile, "ClothingBlue"),
            [typeof(Hulk)] = profile => GetSingleColorProfile(profile, "ClothingGreen"),
            [typeof(DurablePoison)] = profile => GetSingleColorProfile(profile, "ClothingDarkGreen"),
            [typeof(WetHands)] = _ => new Dictionary<string, IProfileClothingItem>
            {
                ["Hands"] = new("Gloves", "ClothingBlue")
            }
        };

    public IReadOnlyDictionary<string, IProfileClothingItem> GetChangedProfile<T>(IProfile profile)
        where T : IDurableBuff
    {
        if (!Profiles.TryGetValue(typeof(T), out var profileFunc))
        {
            throw new ArgumentException($"no profile func found for '{typeof(T).Name}'");
        }

        return profileFunc.Invoke(profile);
    }

    private static Dictionary<string, IProfileClothingItem> GetSingleColorProfile(IProfile profile, string colorName)
    {
        return IProfileExtensions.GetProperties()
            .Select(x => (Key: x, Value: (IProfileClothingItem?)IProfileExtensions.GetField(x).GetValue(profile)))
            .Where(x => x.Item2 is not null)
            .ToDictionary(x => x.Key, x => x.Value.Clone(colorName))!;
    }
}