using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Identity;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Profiles;

internal interface IProfilesService
{
    IProfile GetPlayerProfile(Player player);
    void RequestProfileChanging<T>(Player player) where T : IDurableBuff;
    void RequestProfileRestoring<T>(Player player) where T : IDurableBuff;
}

internal class ProfilesService : IProfilesService
{
    private static readonly Dictionary<Type, int> ProfilesPriority = new()
    {
        [typeof(WetHands)] = 4,
        [typeof(Freeze)] = 3,
        [typeof(DurablePoison)] = 2,
        [typeof(Hulk)] = 1
    };

    private readonly IProfilesRepository _profilesRepository;
    private readonly Dictionary<Player, List<ProfileChange>> _profileChanges = new();

    static ProfilesService()
    {
        if (ProfilesPriority.Keys.ToHashSet().Count != ProfilesPriority.Count)
        {
            throw new Exception("profile priorities cannot be the same");
        }
    }

    public ProfilesService(IProfilesRepository profilesRepository)
    {
        _profilesRepository = profilesRepository;
    }

    public IProfile GetPlayerProfile(Player player)
    {
        if (!_profileChanges.TryGetValue(player, out var changes))
        {
            return player.Profile;
        }

        return GetProfile(player, changes);
    }

    public void RequestProfileChanging<T>(Player player) where T : IDurableBuff
    {
        if (!ProfilesPriority.TryGetValue(typeof(T), out var priority))
        {
            throw new ArgumentException($"cannot found profile priority for '{typeof(T).Name}'");
        }

        if (!_profileChanges.TryGetValue(player, out var changes))
        {
            changes = [];
            _profileChanges.Add(player, changes);
        }

        if (changes.Any(x => x.Priority == priority))
        {
            throw new InvalidOperationException(
                $"profile ({typeof(T).Name}) already changed for this player ({player.Name})");
        }

        changes.Add(new ProfileChange(priority, _profilesRepository.GetChangedProfile<T>(player.Profile)));
        UpdateProfile(player);
    }

    public void RequestProfileRestoring<T>(Player player) where T : IDurableBuff
    {
        if (!ProfilesPriority.TryGetValue(typeof(T), out var priority))
        {
            throw new ArgumentException($"cannot found profile priority for '{typeof(T).Name}'");
        }

        if (!_profileChanges.TryGetValue(player, out var changes))
            return;

        var index = changes.FindIndex(x => x.Priority == priority);
        if (index == -1)
            return;

        changes.RemoveAt(index);

        if (!player.IsInstanceValid())
            return;

        if (changes.Count == 0)
        {
            var profile = player.Profile;
            player.SetInstanceProfile(profile);
            return;
        }

        UpdateProfile(player);
    }

    private void UpdateProfile(Player player)
    {
        if (!player.IsInstanceValid())
            return;

        var changes = _profileChanges[player];
        var profile = GetProfile(player, changes);
        player.SetInstanceProfile(profile);
    }

    private IProfile GetProfile(Player player, List<ProfileChange> changes)
    {
        var originalProfile = player.Profile;
        var profile = new IProfile
        {
            Gender = originalProfile.Gender,
            Name = originalProfile.Name
        };

        foreach (var change in changes.OrderByDescending(x => x.Priority))
        {
            var changedProperties = change.ChangedProperties;
            foreach (var property in changedProperties)
            {
                var name = property.Key;
                var value = property.Value;
                var field = IProfileExtensions.GetField(name);
                var existingValue = field.GetValue(profile);
                if (existingValue is null)
                {
                    field.SetValue(profile, value);
                }
            }
        }

        foreach (var propertyName in IProfileExtensions.GetProperties())
        {
            var field = IProfileExtensions.GetField(propertyName);
            var existingValue = field.GetValue(profile);
            if (existingValue is null)
            {
                var originalValue = field.GetValue(originalProfile);
                field.SetValue(profile, originalValue);
            }
        }

        return profile;
    }

    private record ProfileChange(int Priority, IReadOnlyDictionary<string, IProfileClothingItem> ChangedProperties);
}