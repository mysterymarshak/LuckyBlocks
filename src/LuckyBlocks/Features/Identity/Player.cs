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
    public int UserIdentifier => User.UserIdentifier;
    public string Name => User.Name;
    public IPlayer? Instance => User.GetPlayer();
    public IEnumerable<IImmunity> Immunities => _immunities;
    public IEnumerable<IFinishableBuff> Buffs => _buffs;
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

    public List<IBuff> CloneBuffs(IEnumerable<Type>? exclusions = null)
    {
        return Buffs
            .Where(x => x is ICloneableBuff<IBuff> && exclusions?.All(y => y.IsInstanceOfType(x)) != true)
            .Cast<ICloneableBuff<IBuff>>()
            .Select(x => x.Clone())
            .ToList();
    }

    public void RemoveAllBuffs()
    {
        _buffs
            .ToList()
            .ForEach(x => x.ExternalFinish());
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