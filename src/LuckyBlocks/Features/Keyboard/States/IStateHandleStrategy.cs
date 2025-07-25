using System.Collections.Generic;

namespace LuckyBlocks.Features.Keyboard.States;

internal interface IStateHandleStrategy
{
    IEnumerable<WatchingVirtualKey> WatchingKeys { get; }
    bool Handle();
    void ResetCooldown();
}