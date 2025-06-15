using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

public class GrenadeIndicator
{
    private static readonly IReadOnlyList<Vector2> PaintPattern = new List<Vector2>
    {
        new(0, 3),
        new(0, 2), new(1, 2), new(2, 2)
    };
    
    private readonly IObject _object;
    private readonly IExtendedEvents _extendedEvents;

    public GrenadeIndicator(IObject @object, IExtendedEvents extendedEvents)
    {
        _object = @object;
        _extendedEvents = extendedEvents;
    }

    public void Paint(Color color)
    {
        var drawer = new TextureDrawer(_object, autoDisposeOnDestroy: true, _extendedEvents);
        drawer.Draw(PaintPattern, color, direction => direction == 1 ? new Vector2(-1, 0) : Vector2.Zero);
    }
}