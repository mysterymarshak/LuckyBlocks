using SFDGameScriptInterface;

namespace LuckyBlocks.Entities.TimeStop;

internal interface ITimeStoppedEntity
{
    Vector2 Position { get; }
    void Initialize();
    void ResumeTime();
}