using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class TotemOfUndying : FinishableBuffBase, ICloneableBuff<IFinishableBuff>, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Totem of undying";
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToDeath;

    protected override Color BuffColor => Color.Yellow;

    private readonly BuffConstructorArgs _args;

    private bool _disposed;

    public TotemOfUndying(Player player, BuffConstructorArgs args) : base(player, args.NotificationService,
        args.LifetimeScope)
    {
        _args = args;
    }

    public IFinishableBuff Clone()
    {
        return new TotemOfUndying(Player, _args);
    }

    public override void Run()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        ExtendedEvents.HookOnDead(playerInstance, OnDead, EventHookMode.Default);
        ShowTotemDialogue("Totem of Undying");
    }

    public override void ExternalFinish()
    {
        if (_disposed)
            return;

        CloseDialogue();
        OnFinish();
    }

    private void OnDead(Event<PlayerDeathArgs> @event)
    {
        var args = @event.Args;
        if (args.Removed)
            return;

        var playerInstance = Player.Instance;
        if (playerInstance?.IsValid() == true)
        {
            Player.SetWeapons(Player.WeaponsData, true);
        }

        ShowTotemDialogue("Totem saved you");
        OnFinish();
    }

    private void OnFinish()
    {
        SendFinishNotification();
        Dispose();
    }

    private void ShowTotemDialogue(string message)
    {
        ShowDialogue(message, BuffColor, TimeSpan.FromMilliseconds(2500));
    }

    private void Dispose()
    {
        ExtendedEvents.Clear();

        _disposed = true;
    }
}