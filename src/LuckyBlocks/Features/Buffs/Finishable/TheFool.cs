using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Finishable;

// reference: https://rokuaka.fandom.com/wiki/Original_Magic,_Fool%27s_World
// in anime this card prohibit magic usage in some radius
internal class TheFool : FinishableBuffBase, ICloneableBuff<IFinishableBuff>
{
    public override string Name => "The Fool";
    public override Color BuffColor => ExtendedColors.TheFool;

    private static TimeSpan MagicProhibitionTime => TimeSpan.FromSeconds(15);

    private readonly IMagicService _magicService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly BuffConstructorArgs _args;

    public TheFool(Player player, BuffConstructorArgs args) : base(player, args)
    {
        _magicService = args.MagicService;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
    }

    public IFinishableBuff Clone(Player? player = null)
    {
        return new TheFool(player ?? Player, _args);
    }

    public override void Run()
    {
        ExtendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Default);
        ShowDialogue("The Fool", TimeSpan.FromSeconds(3));
    }

    private void OnKeyInput(Event<IPlayer, VirtualKeyInfo[]> @event)
    {
        var playerInstance = @event.Arg1;
        if (playerInstance != PlayerInstance)
            return;

        if (PlayerInstance.IsWalking && PlayerInstance.IsBlocking)
        {
            ProhibitMagic();
        }
    }

    private void ProhibitMagic()
    {
        _effectsPlayer.PlayHandGleamEffect(PlayerInstance!);
        _magicService.ProhibitMagic(MagicProhibitionTime);
        InternalFinish();
    }
}