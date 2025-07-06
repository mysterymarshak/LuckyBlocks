using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;
using SFDPlayerModifiers = SFDGameScriptInterface.PlayerModifiers;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class DecoyMagic : NonAreaMagicBase
{
    public override string Name => "Decoy magic";

    private const int DecoysCount = 3;
    private const PlayerTeam DecoysTeam = PlayerTeam.Team1;

    private static readonly SFDPlayerModifiers DecoysModifiers = new()
    {
        MeleeDamageTakenModifier = 2f,
        ExplosionDamageTakenModifier = 2f,
        FireDamageTakenModifier = 2f,
        ImpactDamageTakenModifier = 2f,
        ProjectileDamageTakenModifier = 2f,
        ProjectileDamageDealtModifier = 0,
        ProjectileCritChanceDealtModifier = 0,
        MeleeForceModifier = 0,
        MeleeDamageDealtModifier = 0,
        ItemDropMode = 2
    };

    private static TimeSpan DecoysLifeTime => TimeSpan.FromSeconds(10);

    private readonly IGame _game;
    private readonly IDialoguesService _dialoguesService;
    private readonly IExtendedEvents _extendedEvents;
    private readonly List<IPlayer> _decoys;
    private readonly Action<Event> _cachedDecoyDeadEvent;
    private readonly BotBehavior _cachedBotBehavior;

    private Timer? _finishTimer;
    private int _decoysDead;

    public DecoyMagic(Player player, BuffConstructorArgs args) : base(player, args) =>
        (_game, _dialoguesService, _extendedEvents, _decoys, _cachedDecoyDeadEvent, _cachedBotBehavior) = (args.Game,
            args.DialoguesService, LifetimeScope.Resolve<IExtendedEvents>(), [], OnDecoyDead,
            new BotBehavior(true, PredefinedAIType.BotA));

    public override void Cast()
    {
        var wizardInstance = Wizard.Instance!;

        CreateDecoys(DecoysTeam);

        wizardInstance.SetTeam(DecoysTeam);
        _extendedEvents.HookOnDead(wizardInstance, OnDead, EventHookMode.Default);

        _finishTimer = new Timer(DecoysLifeTime, TimeBehavior.TimeModifier, ExternalFinish,
            LifetimeScope.Resolve<IExtendedEvents>());
        _finishTimer.Start();
    }

    private void CreateDecoys(PlayerTeam team)
    {
        var wizardInstance = Wizard.Instance!;
        var position = wizardInstance.GetWorldPosition();
        var profile = wizardInstance.GetProfile();
        var strengthBoostTime = wizardInstance.GetStrengthBoostTime();
        var speedBoostTime = wizardInstance.GetSpeedBoostTime();
        var health = wizardInstance.GetHealth();
        var name = wizardInstance.Name;
        var dialogues = _dialoguesService.CopyDialogues(wizardInstance).ToList();
        wizardInstance.GetUnsafeWeaponsData(out var weaponsData);

        for (var i = 0; i < DecoysCount; i++)
        {
            var bot = _game.CreatePlayer(position);

            bot.SetBotBehavior(_cachedBotBehavior);
            bot.SetBotName(name);
            bot.SetProfile(profile);
            bot.SetTeam(team);
            bot.SetModifiers(DecoysModifiers);
            bot.SetWeapons(weaponsData, true);
            bot.SetStrengthBoostTime(strengthBoostTime);
            bot.SetSpeedBoostTime(speedBoostTime);
            bot.SetHealth(health);
            _dialoguesService.RestoreDialogues(bot, dialogues);
            // decoys dont have user id

            _extendedEvents.HookOnDead(bot, _cachedDecoyDeadEvent, EventHookMode.Default);
            _decoys.Add(bot);
        }
    }

    private void OnDecoyDead(Event @event)
    {
        _decoysDead++;

        if (_decoysDead != _decoys.Count)
            return;

        ExternalFinish();
    }

    private void OnDead(Event @event)
    {
        ExternalFinish();
    }

    protected override void OnFinished()
    {
        Dispose();

        RemoveDecoys();

        var wizardInstance = Wizard.Instance;
        wizardInstance?.SetTeam(PlayerTeam.Independent);
    }

    private void RemoveDecoys()
    {
        _decoys.ForEach(x => x.RemoveDelayed());
        _decoys.Clear();
    }

    private void Dispose()
    {
        _extendedEvents.Clear();
        _finishTimer?.Stop();
    }
}