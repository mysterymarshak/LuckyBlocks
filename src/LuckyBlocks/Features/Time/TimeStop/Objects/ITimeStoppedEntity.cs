using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal interface ITimeStoppedEntity
{
    Vector2 Position { get; }
    void Initialize();
    void ResumeTime();
}