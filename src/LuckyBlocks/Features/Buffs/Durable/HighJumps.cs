using System;
using System.Globalization;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.PlayerModifiers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class HighJumps : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public static readonly SFDGameScriptInterface.PlayerModifiers ModifiedModifiers = new()
    {
        JumpHeight = 2f
    };

    public override string Name => "High jumps";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFall;
    public override Color BuffColor => Color.White;

    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly BuffConstructorArgs _args;

    private SFDGameScriptInterface.PlayerModifiers? _playerModifiers;

    public HighJumps(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
    {
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
    }

    protected override DurableBuffBase CloneInternal(Player player)
    {
        return new HighJumps(player, _args, TimeLeft);
    }

    protected override void OnRunInternal()
    {
        _playerModifiers = PlayerInstance!.GetModifiers();

        EnableBuff();
        UpdateDialogue();
    }

    protected override void OnFinishInternal()
    {
        DisableBuff();
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue();
        ShowApplyAgainChatMessage("a strong-legs man");
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
        ShowPersistentDialogue("HIGH JUMPS");
    }
}