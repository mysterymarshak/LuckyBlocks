using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities.TimeStop;

internal class TimeStoppedLiquid : ITimeStoppedEntity
{
    public Vector2 Position => _object.GetWorldPosition();

    private readonly IObject _object;
    private readonly IGame _game;

    private IObject? _block;
    
    public TimeStoppedLiquid(IObject @object, IGame game)
        => (_object, _game) = (@object, game);
    
    public void Initialize()
    {
        _block = _game.CreateObject("InvisibleBlock", _object.GetWorldPosition() + new Vector2(0, 4));
        _block.SetSizeFactor(_object.GetSizeFactor());
    }

    public void ResumeTime()
    {
        _block!.RemoveDelayed();
    }
}