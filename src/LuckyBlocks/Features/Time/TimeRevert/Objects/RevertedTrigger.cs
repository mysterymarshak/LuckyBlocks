using System.Collections.Generic;
using System.Linq;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedTrigger : RevertedStaticObject
{
    private readonly bool _isEnabled;
    private readonly IObjectTrigger[] _triggerObjects;

    public RevertedTrigger(IObjectTrigger trigger) : base(trigger)
    {
        _isEnabled = trigger.IsEnabled;
        _triggerObjects = trigger.GetTriggerObjects();
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        var trigger = (IObjectTrigger)Object;

        if (trigger.IsEnabled != _isEnabled)
        {
            trigger.SetEnabled(_isEnabled);
        }

        if (!trigger.GetTriggerObjects().SequenceEqual(_triggerObjects))
        {
            trigger.SetTriggerObjects(_triggerObjects);
        }
    }
}