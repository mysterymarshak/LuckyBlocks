using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities;

internal class Text
{
    private const int ANIMATION_STEPS = 85;
    private const int ANIMATION_OFFSET_MAX_Y = 20;

    private readonly IObjectText _text;
    private readonly Vector2 _startPosition;
    private readonly Events.UpdateCallback _updateCallback;

    private int _animationStep;

    private Text(IObjectText text, TimeSpan displayTime)
    {
        _text = text;
        _startPosition = text.GetWorldPosition();
        _updateCallback = Events.UpdateCallback.Start(Update, (uint)(displayTime.TotalMilliseconds / ANIMATION_STEPS));
    }

    public static Text CreateAnimatedText(string message, Color color, TimeSpan displayTime, IPlayer parent, IGame game)
    {
        var startPosition = parent.GetWorldPosition() + new Vector2(1, 20);
        var textObject = CreateTextObject(message, color, startPosition, game);
        return new Text(textObject, displayTime);
    }

    public void Remove()
    {
        _updateCallback.Stop();
        _text.Remove();
    }

    private static IObjectText CreateTextObject(string message, Color color, Vector2 position, IGame game)
    {
        var text = (game.CreateObject("Text", position) as IObjectText)!;

        text.SetText(message);
        text.SetTextAlignment(TextAlignment.Middle);
        text.SetTextColor(color);
        text.SetTextScale(0.5f);

        return text;
    }

    private void Update(float e)
    {
        if (_animationStep == ANIMATION_STEPS)
        {
            Remove();
            return;
        }

        _text.SetWorldPosition(_startPosition +
                               new Vector2(0, ANIMATION_OFFSET_MAX_Y * _animationStep / ANIMATION_STEPS));

        _animationStep++;
    }
}