using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class StrongMan : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public static readonly SFDGameScriptInterface.PlayerModifiers ModifiedModifiers = new()
    {
        ProjectileDamageDealtModifier = 100,
        MeleeDamageDealtModifier = 100,
        MeleeForceModifier = 10,
        MeleeStunImmunity = 1,
        ThrowForce = 3f
    };

    public override string Name => "Strong man";
    public override TimeSpan Duration => TimeSpan.FromSeconds(5);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToWind;
    public override Color BuffColor => ExtendedColors.ImperialRed;

    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly BuffConstructorArgs _args;

    private SFDGameScriptInterface.PlayerModifiers? _playerModifiers;

    public StrongMan(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
    {
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
        _args = args;
    }

    protected override DurableBuffBase CloneInternal(Player player)
    {
        return new StrongMan(player, _args, TimeLeft);
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue();

        PlayerInstance!.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);

        ShowChatMessage($"You are strong again for {TimeLeft.TotalSeconds}s");
    }

    protected override void OnFinishInternal()
    {
        DisableBuff();
    }

    protected override void OnRunInternal()
    {
        _playerModifiers = PlayerInstance!.GetModifiers();

        EnableBuff();
        UpdateDialogue();
    }

    private void EnableBuff()
    {
        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
        PlayerInstance!.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
    }

    private void UpdateDialogue()
    {
        ShowPersistentDialogue("STRONG");
    }
}