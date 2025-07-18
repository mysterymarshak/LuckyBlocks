using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class TotemOfUndying : FinishableBuffBase, ICloneableBuff<IFinishableBuff>, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Totem of undying";
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToDeath;
    public override Color BuffColor => ExtendedColors.TotemOfUndying;

    private readonly BuffConstructorArgs _args;

    public TotemOfUndying(Player player, BuffConstructorArgs args) : base(player, args)
    {
        _args = args;
    }

    public IFinishableBuff Clone(Player? player = null)
    {
        return new TotemOfUndying(player ?? Player, _args);
    }

    public override void Run()
    {
        ExtendedEvents.HookOnDead(PlayerInstance!, OnDead, EventHookMode.Default);
        ShowTotemDialogue("Totem of Undying");
    }

    private void OnDead(Event<PlayerDeathArgs> @event)
    {
        var args = @event.Args;
        if (args.Removed)
            return;

        if (Player.IsInstanceValid())
        {
            Player.SetWeapons(Player.WeaponsData, true);
        }

        ShowTotemDialogue("Totem saved you", true);
        InternalFinish();
    }

    private void ShowTotemDialogue(string message, bool ignoreFinish = false)
    {
        ShowDialogue(message, TimeSpan.FromSeconds(3), BuffColor, ignoreFinish: ignoreFinish);
    }
}