using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class Shield : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public static readonly SFDGameScriptInterface.PlayerModifiers ModifiedModifiers = new()
    {
        ExplosionDamageTakenModifier = 0,
        FireDamageTakenModifier = 0,
        ImpactDamageTakenModifier = 0,
        MeleeDamageTakenModifier = 0,
        ProjectileDamageTakenModifier = 0,
        ProjectileCritChanceTakenModifier = 0
    };

    public override string Name => "Shield";
    public override TimeSpan Duration => TimeSpan.FromSeconds(7);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.FullDamageImmunity;
    public override Color BuffColor => Color.Blue;
    public override Color ChatColor => ExtendedColors.ChatBlue;

    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly BuffConstructorArgs _args;

    private SFDGameScriptInterface.PlayerModifiers? _playerModifiers;

    public Shield(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
    {
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
    }

    protected override DurableBuffBase CloneInternal(Player player)
    {
        return new Shield(player, _args, TimeLeft);
    }

    protected override void OnRunInternal()
    {
        _playerModifiers = PlayerInstance!.GetModifiers();

        EnableBuff();
        UpdateDialogue();
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue();
        ShowApplyAgainChatMessage("under the shield");
    }

    protected override void OnFinishInternal()
    {
        DisableBuff();
    }

    private void EnableBuff()
    {
        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
    }

    private void UpdateDialogue()
    {
        ShowPersistentDialogue("SHIELD");
    }
}