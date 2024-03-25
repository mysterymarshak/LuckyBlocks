using System.Collections.Generic;

namespace LuckyBlocks.Wayback;

internal readonly record struct WaybackSnapshot(float Time, List<IWaybackObject> Objects);