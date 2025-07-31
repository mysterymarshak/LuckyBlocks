using LuckyBlocks.Features.Objects;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal class TimeStoppedMine : TimeStoppedDynamicObject
{
    private readonly IObjectMineThrown _realMine;
    private readonly IGame _game;
    private readonly IMappedObjectsService _mappedObjectsService;

    private float _dudChance;
    private bool _wasAlreadyDud;

    public TimeStoppedMine(IObjectMineThrown mine, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents, IMappedObjectsService mappedObjectsService) : base(mine, game, effectsPlayer,
        extendedEvents)
    {
        _realMine = mine;
        _game = game;
        _mappedObjectsService = mappedObjectsService;
    }

    protected override void InitializeInternal()
    {
        base.InitializeInternal();

        if (_realMine.ExplosionResultedInDud)
        {
            _wasAlreadyDud = true;
            return;
        }

        _dudChance = _realMine.GetDudChance();
        _realMine.SetDudChance(1f);
    }

    protected override void ResumeTimeInternal()
    {
        base.ResumeTimeInternal();

        if (!_realMine.ExplosionResultedInDud || _wasAlreadyDud)
            return;

        _realMine.Remove();

        var newMine = (IObjectMineThrown)_game.CreateObject(_realMine.Name);
        newMine.SetDudChance(_dudChance);

        Object = newMine;
        _mappedObjectsService.UpdateIfMapped(_realMine.UniqueId, Object);
    }
}