using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.Profiles;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class Hulk : DurableBuffBase
{
    public static readonly SFDGameScriptInterface.PlayerModifiers ModifiedModifiers = new()
    {
        SizeModifier = 2,
        RunSpeedModifier = 0.75f,
        SprintSpeedModifier = 0.75f,
        MeleeDamageDealtModifier = 3,
        MeleeForceModifier = 10,
        ExplosionDamageTakenModifier = 0.8f,
        FireDamageTakenModifier = 0.8f,
        ImpactDamageTakenModifier = 0.8f,
        MeleeDamageTakenModifier = 0.8f,
        ProjectileCritChanceTakenModifier = 0.8f,
        ProjectileDamageTakenModifier = 0.8f,
        ClimbingSpeed = 0.75f,
        ThrowForce = 3f
    };

    public override string Name => "Hulk";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);
    public override Color BuffColor => ExtendedColors.Emerald;

    private readonly IProfilesService _profilesService;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly BuffConstructorArgs _args;

    private SFDGameScriptInterface.PlayerModifiers? _playerModifiers;

    public Hulk(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
    {
        _profilesService = args.ProfilesService;
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
    }

    protected override void OnRunInternal()
    {
        _playerModifiers = PlayerInstance!.GetModifiers();

        EnableBuff();
        UpdateDialogue();
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new Hulk(Player, _args, TimeLeft);
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue();

        PlayerInstance!.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);

        ShowChatMessage($"You are a hulk again for {TimeLeft.TotalSeconds}s");
    }

    protected override void OnFinishInternal()
    {
        DisableBuff();
    }

    private void EnableBuff()
    {
        _profilesService.RequestProfileChanging<Hulk>(Player);
        PlayerInstance!.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);

        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
        _profilesService.RequestProfileRestoring<Hulk>(Player);
    }

    private void UpdateDialogue()
    {
        ShowPersistentDialogue("HULK");
    }
}