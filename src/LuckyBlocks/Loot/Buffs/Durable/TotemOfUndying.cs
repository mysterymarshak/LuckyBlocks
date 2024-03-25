using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class TotemOfUndying : FinishableBuffBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Totem of undying";
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToDeath;

    protected override Color BuffColor => Color.Yellow;

    private bool _disposed;
    private IPlayer? _savedPlayerInstance;

    public TotemOfUndying(Player player, BuffConstructorArgs args) : base(player, args.NotificationService,
        args.LifetimeScope)
    {
    }

    public override void Run()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        _savedPlayerInstance = playerInstance;

        ExtendedEvents.HookOnDead(playerInstance, OnDead, EventHookMode.Default);
        ShowTotemDialogue();
    }

    public override void ExternalFinish()
    {
        if (!_disposed)
        {
            CloseDialogue();
        }

        SendFinishNotification();

        Dispose();
    }

    private void OnDead(Event<PlayerDeathArgs> @event)
    {
        var args = @event.Args;
        if (args.Removed)
            return;

        var playerInstance = Player.Instance;
        if (_savedPlayerInstance?.IsValid() == true && playerInstance?.IsValid() == true)
        {
            _savedPlayerInstance.GetUnsafeWeaponsData(out var weaponsData);
            _savedPlayerInstance.RemoveAllWeapons();
            playerInstance.SetWeapons(weaponsData, true);
        }

        ShowTotemDialogue();
        Dispose();
    }

    private void ShowTotemDialogue()
    {
        ShowDialogue("Totem of Undying", BuffColor, TimeSpan.FromMilliseconds(2500), default, true);
    }

    private void Dispose()
    {
        _disposed = true;
        ExtendedEvents.Clear();
    }
}