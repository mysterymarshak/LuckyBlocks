using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Loot.Buffs;
using LuckyBlocks.Wayback;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities;

internal class Player : IStately<PlayerState>
{
    public int UserIdentifier => User.UserIdentifier;
    public string Name => User.Name;
    public IPlayer? Instance => User.GetPlayer();
    public IEnumerable<IImmunity> Immunities => _immunities;
    public IEnumerable<IFinishableBuff> Buffs => _buffs;
    public IProfile Profile { get; }
    public IUser User { get; }
    public PlayerModifiers ModifiedModifiers { get; set; }
    
    private readonly List<IFinishableBuff> _buffs;
    private readonly List<IImmunity> _immunities;

    public Player(IUser user)
    {
        User = user;
        
        ArgumentWasNullException.ThrowIfNull(Instance);
        
        Profile = Instance.GetProfile();
        ModifiedModifiers = new PlayerModifiers();
        _buffs = new();
        _immunities = new();
    }

    public void AddBuff(IBuff buff)
    {
        if (buff is IStackableBuff)
        {
            if (_buffs.FirstOrDefault(x => x.Name == buff.Name) is IStackableBuff alreadyExistingBuff)
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

    public void RemoveAllBuffs()
    {
        _buffs
            .ToList()
            .ForEach(x => x.ExternalFinish());
    }

    public IState GetState()
    {
        return new PlayerState(this);
    }

    public void RestoreFromState(IState state)
    {
        RemoveAllBuffs();
        
        foreach (var buff in ((PlayerState)state).Buffs)
        {
            AddBuff(buff);
        }
    }
}

internal class PlayerState : IState
{
    public IEnumerable<ICloneableBuff<IBuff>> Buffs { get; }

    public PlayerState(Player player)
    {
        Buffs = player.Buffs
            .OfType<ICloneableBuff<IBuff>>()
            .ToList();
    }
}