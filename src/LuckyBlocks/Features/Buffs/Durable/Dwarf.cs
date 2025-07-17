using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class Dwarf : DurableBuffBase
{
    public static readonly SFDGameScriptInterface.PlayerModifiers ModifiedModifiers = new()
    {
        MeleeForceModifier = 0.7f,
        SizeModifier = 0.5f,
        RunSpeedModifier = 2f,
        SprintSpeedModifier = 2f,
        EnergyRechargeModifier = 2f,
        ClimbingSpeed = 2f
    };

    public override string Name => "Dwarf";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);
    public override Color BuffColor => ExtendedColors.Emerald;

    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly BuffConstructorArgs _args;

    private SFDGameScriptInterface.PlayerModifiers? _playerModifiers;

    public Dwarf(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
    {
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new Dwarf(Player, _args, TimeLeft);
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
        ShowChatMessage($"You're a dwarf again for {TimeLeft.TotalSeconds}s");
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
        ShowPersistentDialogue("DWARF");
    }
}