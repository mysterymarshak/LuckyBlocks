using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities;

internal class ShockedObject
{
    public const double ELEMENTARY_CHARGE = 50;

    public double Charge => TimeLeft.TotalMilliseconds;
    public int ObjectId => _object.UniqueId;
    public string Name => _object.Name;
    public TimeSpan TimeLeft { get; set; }

    private bool IsShocked { get; set; }

    private readonly IObject _object;
    private readonly TimeSpan _shockDuration;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly IReadOnlyList<Vector2> _collisionVectors;

    private float _elapsedFromPreviousEffect;

    public ShockedObject(IObject @object, TimeSpan shockDuration, IEffectsPlayer effectsPlayer, IGame game)
    {
        _object = @object;
        _shockDuration = shockDuration;
        _effectsPlayer = effectsPlayer;
        _game = game;
        _collisionVectors = Enumerable.Range(0, 8)
            .Select(x => x * 45)
            .Select(x => x * Math.PI / 180)
            .Select(x =>
                new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) *
                (@object.GetAABB().GetDiagonalLength() / 2 + 3))
            .ToList();
    }

    public void Initialize()
    {
        IsShocked = true;
        TimeLeft = _shockDuration;
    }

    public IEnumerable<IObject> Update(float elapsed, bool isTimeStopped)
    {
        var touchedObjects = Enumerable.Empty<IObject>();

        if (!isTimeStopped)
        {
            TimeLeft = TimeSpan.FromMilliseconds(Math.Max(0, TimeLeft.TotalMilliseconds - elapsed));

            var position = _object.GetWorldPosition();
            touchedObjects = _collisionVectors
                .Select(x => _game.RayCast(position, position + x, default))
                .SelectMany(x => x)
                .Where(x => x.Hit && x.HitObject.GetBodyType() != BodyType.Static &&
                            x.HitObject.GetPhysicsLayer() == PhysicsLayer.Active)
                .Select(x => x.HitObject)
                .ToList();

#if DEBUG
            if (_game.IsEditorTest)
            {
                foreach (var collisionVector in _collisionVectors)
                {
                    _game.DrawLine(position, position + collisionVector, Color.Red);
                }
            }
#endif
        }

        _elapsedFromPreviousEffect += elapsed;

        if (_elapsedFromPreviousEffect > 300f)
        {
            _effectsPlayer.PlayEffect(EffectName.Electric, _object.GetWorldPosition());
            _elapsedFromPreviousEffect = 0;
        }

        return touchedObjects;
    }

    public void Dispose()
    {
        IsShocked = false;
    }
}