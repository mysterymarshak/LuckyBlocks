using SFDGameScriptInterface;

namespace LuckyBlocks.Entities.TimeStop;

// source idea: https://steamcommunity.com/sharedfiles/filedetails/?id=2061774989

internal class TimeStoppedBackgroundObject : ITimeStoppedEntity
{
    public Vector2 Position => _object.GetWorldPosition();

    private static readonly string[] Colors = ["BgGray", "White", string.Empty];

    private readonly IObject _object;
    private readonly string[] _colors;

    public TimeStoppedBackgroundObject(IObject @object)
        => (_object, _colors) = (@object, @object.GetColors());

    public void Initialize()
    {
        _object.SetColors(Colors);
    }

    public void ResumeTime()
    {
        _object.SetColors(_colors);
    }
}