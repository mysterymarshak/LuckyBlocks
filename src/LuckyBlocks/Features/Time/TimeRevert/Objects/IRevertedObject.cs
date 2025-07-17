using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal interface IRevertedObject : IRevertedEntity
{
    int OldObjectId { get; }
    string Name { get; }
    IObject Object { get; }
    int Restore(IGame game, Dictionary<int, int>? objectsMap = null);
}