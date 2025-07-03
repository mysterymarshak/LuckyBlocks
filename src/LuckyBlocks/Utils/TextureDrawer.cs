using System;
using System.Collections.Generic;
using LuckyBlocks.Extensions;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

[Inject]
internal class TextureDrawer
{
    [InjectGame]
    private static IGame Game { get; set; }

    private readonly IObject _object;
    private readonly List<IObjectText> _texts;
    private readonly IObjectWeldJoint _drawingWeldJoint;
    private readonly IEventSubscription? _objectDestroyed;

    public TextureDrawer(IObject @object, bool autoDisposeOnDestroy = false, IExtendedEvents? extendedEvents = null)
    {
        _object = @object;
        _texts = new();
        _drawingWeldJoint = (Game.CreateObject("WeldJoint", @object.GetWorldPosition()) as IObjectWeldJoint)!;
        _drawingWeldJoint.AddTargetObject(@object);

        if (autoDisposeOnDestroy)
        {
            _objectDestroyed = extendedEvents?.HookOnDestroyed(@object, OnObjectDestroyed, EventHookMode.Default);
        }
    }

    public void Draw(IEnumerable<Vector2> pattern, Color color,
        Func<int, Vector2>? offsetByFaceDirectionDelegate = default)
    {
        var texts = new List<IObjectText>();
        var position = _object.GetWorldPosition();

        foreach (var offset in pattern)
        {
            var text = (Game.CreateObject("Text",
                position + offset + offsetByFaceDirectionDelegate?.Invoke(_object.GetFaceDirection()) ??
                Vector2.Zero) as IObjectText)!;

            text.SetMass(float.MinValue);
            text.SetText(".");
            text.SetTextColor(color);
            text.SetTextAlignment(TextAlignment.Middle);
            text.SetBodyType(BodyType.Dynamic);

            texts.Add(text);
            _drawingWeldJoint.AddTargetObject(text);
        }

        _texts.AddRange(texts);
    }

    public void Dispose()
    {
        _drawingWeldJoint.RemoveDelayed();

        foreach (var text in _texts)
        {
            text.RemoveDelayed();
        }

        _objectDestroyed?.Dispose();
    }

    private void OnObjectDestroyed(Event @event)
    {
        Dispose();
    }
}