using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal interface IAreaMagic : IMagic
{
    event Action<IAreaMagic>? Iterate;
    AreaMagicType Type { get; }
    Vector2 AreaSize { get; }
    TimeSpan PropagationTime { get; }
    int IterationsCount { get; }
    int Direction { get; }
    void Reflect();
    Area GetCurrentIteration();
    Area GetFullArea();
    void Cast(Area fullArea, Area iterationArea);
    void PlayEffects(Area area);
}