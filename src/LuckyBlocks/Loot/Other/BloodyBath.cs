using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class BloodyBath : ILoot
{
    public Item Item => Item.BloodyBath;
    public string Name => "Bloody bath";

    private readonly INotificationService _notificationService;
    private readonly IGame _game;
    private readonly ILifetimeScope _lifetimeScope;

    private List<IObject>? _barrels;

    public BloodyBath(LootConstructorArgs args)
        => (_notificationService, _game, _lifetimeScope) =
            (args.NotificationService, args.Game, args.LifetimeScope.BeginLifetimeScope());

    public void Run()
    {
        SpawnBurningBarrels();

        var extendedEvents = _lifetimeScope.Resolve<IExtendedEvents>();
        var bloodCreationTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(200), TimeBehavior.TimeModifier, CreateBloodEffect,
            _lifetimeScope.Dispose, 15,
            extendedEvents);
        bloodCreationTimer.Start();

        _notificationService.CreateChatNotification("GIVE THEM A BLOODBATH!11!1", Color.Red);
    }

    private void SpawnBurningBarrels()
    {
        var cameraArea = _game.GetCameraArea();
        var topLeft = cameraArea.TopLeft;
        var topRight = cameraArea.TopRight;
        var length = (topRight - topLeft).Length();

        const int distanceBetweenBarrels = 20;
        var barrelsCount = (int)Math.Round(length / distanceBetweenBarrels);

        _barrels = new List<IObject>(barrelsCount);

        for (var i = 0; i < barrelsCount; i++)
        {
            var position = new Vector2(topLeft.X + (distanceBetweenBarrels * i), topLeft.Y);

            var barrel = _game.CreateObject("BarrelExplosive", position);
            barrel.SetMaxFire();

            _barrels.Add(barrel);
        }
    }

    private void CreateBloodEffect()
    {
        if (!_barrels!.Any())
        {
            return;
        }

        _barrels!.RemoveAll(x => x.IsRemoved || x.DestructionInitiated);

        foreach (var barrel in _barrels)
        {
            _game.PlayEffect(EffectName.Blood, barrel.GetWorldPosition());
        }
    }
}