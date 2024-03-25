using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal interface IAreaMagic : IMagic
{
    AreaMagicType Type { get; }
    Vector2 AreaSize { get; }
    TimeSpan PropagationTime { get; }
    int IterationsCount { get; }
    int Direction { get; }
    void Reflect();
    void Cast(Area area);
    void PlayEffects(Area area);
}