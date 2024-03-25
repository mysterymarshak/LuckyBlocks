using System;

namespace LuckyBlocks.Features.Magic;

[Flags]
public enum MagicCollisionResult
{
    None = 0,

    WasAbsorbed = 1,

    Absorb = 2,

    Explosion = 4,
    
    Reflect = 8,
    
    WasReflected = 16
}