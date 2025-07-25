using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal readonly record struct DecoyInstance(
    bool IsDead,
    IPlayer Instance,
    string Name,
    IProfile Profile,
    WeaponsData WeaponsData,
    Vector2 Position,
    Vector2 Velocity,
    float StrengthBoostTime,
    float SpeedBoostTime,
    float Health,
    List<Dialogue> Dialogues,
    List<ICloneableBuff<IBuff>> Buffs);

internal class DecoyMagic : NonAreaMagicBase
{
    public override string Name => "Decoy magic";

    private const int DecoysCount = 3;
    private const PlayerTeam DecoysTeam = PlayerTeam.Team1;

    private static TimeSpan DecoysLifeTime => TimeSpan.FromSeconds(10);

    private readonly IGame _game;
    private readonly IDialoguesService _dialoguesService;
    private readonly IIdentityService _identityService;
    private readonly IBuffsService _buffsService;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly MagicConstructorArgs _args;
    private readonly List<IPlayer> _decoys = [];
    private readonly List<DecoyInstance>? _copiedDecoys;
    private readonly TimeSpan _timeLeft;
    private readonly BotBehavior _cachedBotBehavior;

    private Timer? _finishTimer;
    private int _decoysDead;

    public DecoyMagic(Player player, MagicConstructorArgs args, List<DecoyInstance>? copiedDecoys = null,
        TimeSpan timeLeft = default) : base(player,
        args)
    {
        _game = args.Game;
        _dialoguesService = args.DialoguesService;
        _identityService = args.IdentityService;
        _buffsService = args.BuffsService;
        _weaponPowerupsService = args.WeaponPowerupsService;
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
        _copiedDecoys = copiedDecoys;
        _timeLeft = timeLeft;
        _cachedBotBehavior = new BotBehavior(true, PredefinedAIType.BotA);
    }

    public override void Cast()
    {
        var wizardInstance = Wizard.Instance!;

        wizardInstance.SetTeam(DecoysTeam);
        ExtendedEvents.HookOnDead(wizardInstance, OnWizardDead, EventHookMode.Default);

        if (_copiedDecoys is null)
        {
            CreateDecoys();
            _finishTimer = new Timer(DecoysLifeTime, TimeBehavior.TimeModifier, ExternalFinish, ExtendedEvents);
        }
        else
        {
            RestoreDecoys();
            _finishTimer = new Timer(_timeLeft, TimeBehavior.TimeModifier, ExternalFinish, ExtendedEvents);
        }

        Wizard.ProfileChanged += OnProfileChanged;

        _finishTimer.Start();
    }

    public override MagicBase Copy()
    {
        var copiedDecoys = CopyDecoys();
        return new DecoyMagic(Wizard, _args, copiedDecoys, _finishTimer!.TimeLeft) { _decoysDead = _decoysDead };
    }

    protected override void OnFinishInternal()
    {
        Dispose();
        RemoveDecoys();

        var wizardInstance = Wizard.Instance;
        wizardInstance?.SetTeam(PlayerTeam.Independent);
        Wizard.ProfileChanged -= OnProfileChanged;
    }

    private void CreateDecoys()
    {
        var wizardInstance = Wizard.Instance!;
        var position = wizardInstance.GetWorldPosition();
        var profile = wizardInstance.GetProfile();
        var strengthBoostTime = wizardInstance.GetStrengthBoostTime();
        var speedBoostTime = wizardInstance.GetSpeedBoostTime();
        var health = wizardInstance.GetHealth();
        var name = wizardInstance.Name;
        var dialogues = _dialoguesService.CopyDialogues(wizardInstance).ToList();

        for (var i = 0; i < DecoysCount; i++)
        {
            var bot = _game.CreatePlayer(position);

            bot.SetBotBehavior(_cachedBotBehavior);
            bot.SetBotName(name);
            bot.SetProfile(profile);
            bot.SetTeam(DecoysTeam);
            bot.SetModifiers(_playerModifiersService.DecoysModifiers);
            bot.SetStrengthBoostTime(strengthBoostTime);
            bot.SetSpeedBoostTime(speedBoostTime);
            bot.SetHealth(health);

            var fakePlayer = _identityService.RegisterFake(Wizard, bot);
            var weaponsDataCopy = _weaponPowerupsService.CreateWeaponsDataCopy(Wizard);
            _weaponPowerupsService.RestoreWeaponsDataFromCopy(fakePlayer, weaponsDataCopy, false);

            var clonedBuffs = _buffsService.CloneBuffs(Wizard, playerToBind: fakePlayer);
            foreach (var buff in clonedBuffs)
            {
                _buffsService.TryAddBuff(buff, fakePlayer, false);
            }

            _dialoguesService.RestoreDialogues(bot, dialogues);

            ExtendedEvents.HookOnDead(bot, OnDecoyDead, EventHookMode.Default);
            _decoys.Add(bot);
        }
    }

    private void RestoreDecoys()
    {
        foreach (var decoyCopy in _copiedDecoys!)
        {
            var decoyInstance = decoyCopy.Instance;

            if (!decoyInstance.IsValid())
            {
                decoyInstance = _game.CreatePlayer(decoyCopy.Position);
                decoyInstance.SetBotName(decoyCopy.Name);
                decoyInstance.SetProfile(decoyCopy.Profile);
                decoyInstance.SetTeam(DecoysTeam);
                decoyInstance.SetBotBehavior(_cachedBotBehavior);
            }
            else
            {
                decoyInstance.SetWorldPosition(decoyCopy.Position);
            }

            if (!decoyCopy.IsDead)
            {
                var fakePlayer = _identityService.RegisterFake(Wizard, decoyInstance);
                _weaponPowerupsService.RestoreWeaponsDataFromCopy(fakePlayer, decoyCopy.WeaponsData, false);
                decoyInstance.SetStrengthBoostTime(decoyCopy.StrengthBoostTime);
                decoyInstance.SetSpeedBoostTime(decoyCopy.SpeedBoostTime);

                foreach (var buff in decoyCopy.Buffs)
                {
                    var clonedBuff = buff.Clone(fakePlayer);
                    _buffsService.TryAddBuff(clonedBuff, fakePlayer, false);
                }

                _dialoguesService.RestoreDialogues(decoyInstance, decoyCopy.Dialogues);

                ExtendedEvents.HookOnDead(decoyInstance, OnDecoyDead, EventHookMode.Default);
            }

            decoyInstance.SetModifiers(_playerModifiersService.DecoysModifiers);
            decoyInstance.SetHealth(decoyCopy.Health);
            decoyInstance.SetLinearVelocity(decoyCopy.Velocity);

            _decoys.Add(decoyInstance);
        }
    }

    private List<DecoyInstance> CopyDecoys()
    {
        var copiedDecoys = new List<DecoyInstance>(_decoys.Count);

        foreach (var decoyInstance in _decoys)
        {
            if (!decoyInstance.IsValid())
                continue;

            var decoy = _identityService.GetPlayerByInstance(decoyInstance);
            var weaponsDataCopy = _weaponPowerupsService.CreateWeaponsDataCopy(decoy);
            var decoyCopy = new DecoyInstance(decoyInstance.IsDead, decoyInstance, decoyInstance.Name,
                decoyInstance.GetProfile(), weaponsDataCopy, decoyInstance.GetWorldPosition(),
                decoyInstance.GetLinearVelocity(), decoyInstance.GetStrengthBoostTime(),
                decoyInstance.GetSpeedBoostTime(), decoyInstance.GetHealth(),
                _dialoguesService.CopyDialogues(decoyInstance).ToList(), _buffsService.CloneBuffs(Wizard));
            copiedDecoys.Add(decoyCopy);
        }

        return copiedDecoys;
    }

    private void OnProfileChanged(IProfile profile)
    {
        foreach (var decoy in _decoys)
        {
            if (decoy.IsValid())
            {
                decoy.SetProfile(profile);
            }
        }
    }

    private void OnDecoyDead(Event<IPlayer, PlayerDeathArgs> @event)
    {
        _decoysDead++;

        var decoyInstance = @event.Arg1;
        decoyInstance.RemoveDelayed();

        if (_decoysDead != _decoys.Count)
            return;

        ExternalFinish();
    }

    private void OnWizardDead(Event @event)
    {
        ExternalFinish();
    }

    private void RemoveDecoys()
    {
        _decoys.ForEach(x => x.RemoveDelayed());
        _decoys.Clear();
    }

    private void Dispose()
    {
        _finishTimer?.Stop();
    }
}