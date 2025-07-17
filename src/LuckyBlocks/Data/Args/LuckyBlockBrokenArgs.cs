using LuckyBlocks.Loot;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Args;

internal record LuckyBlockBrokenArgs(
    int LuckyBlockId,
    Vector2 Position,
    bool IsPlayer,
    int PlayerId,
    bool ShouldHandle,
    Item PredefinedItem);