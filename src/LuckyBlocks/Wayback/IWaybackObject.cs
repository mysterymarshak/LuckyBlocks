using SFDGameScriptInterface;

namespace LuckyBlocks.Wayback;

internal interface IWaybackObject
{
    IObject Object { get; }
    void Restore(IGame game);
}