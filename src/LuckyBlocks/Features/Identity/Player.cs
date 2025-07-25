using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Wizards;
using LuckyBlocks.Features.Immunity;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal class Player
{
    public event Action<IProfile>? ProfileChanged;

    public int UserIdentifier => User.UserIdentifier;
    public string Name => User.Name;
    public IPlayer? Instance => User.GetPlayer();
    public IEnumerable<IImmunity> Immunities => _immunities;
    public IReadOnlyCollection<IFinishableBuff> Buffs => _buffs;
    public IWizard? WizardBuff => _buffs.OfType<IWizard>().SingleOrDefault();
    public IProfile Profile { get; }
    public IUser User { get; }
    public SFDGameScriptInterface.PlayerModifiers ModifiedModifiers { get; set; }
    public WeaponsData WeaponsData { get; private set; }

    private readonly List<IFinishableBuff> _buffs;
    private readonly List<IImmunity> _immunities;

    public Player(IUser user)
    {
        User = user;
        Profile = user.GetProfile();
        ModifiedModifiers = new SFDGameScriptInterface.PlayerModifiers();
        _buffs = [];
        _immunities = [];
        WeaponsData = Instance?.CreateWeaponsData()!;
    }

    public void AddBuff(IBuff buff)
    {
        if (buff is IStackableBuff)
        {
            if (_buffs.FirstOrDefault(x => x.GetType() == buff.GetType()) is IStackableBuff alreadyExistingBuff)
            {
                alreadyExistingBuff.ApplyAgain(buff);
                return;
            }
        }

        buff.Run();

        if (buff is IFinishableBuff finishableBuff)
        {
            _buffs.Add(finishableBuff);
        }
    }

    public void RemoveBuff(IFinishableBuff buff)
    {
        _buffs.Remove(buff);
    }

    public void AddImmunity(IImmunity immunity)
    {
        if (immunity is IApplicableImmunity applicableImmunity)
        {
            applicableImmunity.Apply();
        }

        _immunities.Add(immunity);
    }

    public void RemoveImmunity(IImmunity immunity)
    {
        _immunities.Remove(immunity);
    }

    public void SetInstanceProfile(IProfile profile)
    {
        if (!this.IsInstanceValid())
            return;

        Instance!.SetProfile(profile);
        ProfileChanged?.Invoke(profile);
    }

    public void SetWeaponsData(WeaponsData weaponsData)
    {
        WeaponsData = weaponsData;
        InvalidateWeaponsDataOwner();
    }

    public void InvalidateWeaponsDataOwner()
    {
        WeaponsData.Owner = Instance!;
    }
}