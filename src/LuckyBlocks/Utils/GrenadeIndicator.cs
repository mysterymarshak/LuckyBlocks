using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

internal class GrenadeIndicator
{
    private static readonly IReadOnlyList<Vector2> PaintPattern = new List<Vector2>
    {
        new(0, 3),
        new(0, 2), new(1, 2), new(2, 2)
    };

    private readonly IObject _grenade;
    private readonly IExtendedEvents _extendedEvents;

    public GrenadeIndicator(IObject grenade, IExtendedEvents extendedEvents)
    {
        _grenade = grenade;
        _extendedEvents = extendedEvents;
    }

    public void Paint(Color color)
    {
        var drawer = new TextureDrawer(_grenade, autoDisposeOnDestroy: true, _extendedEvents);
        drawer.Draw(PaintPattern, color, direction => direction == 1 ? new Vector2(-1, 0) : Vector2.Zero);
    }
}