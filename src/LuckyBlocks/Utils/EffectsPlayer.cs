using System;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

internal interface IEffectsPlayer
{
    void PlaySloMoEffect(TimeSpan slowMoDuration);
    void PlayShakeEffect(float intensity, TimeSpan duration);
    void PlayShakeSoundEffect();
    void PlayHandGleamEffect(IPlayer playerInstance);
    void PlayLostEffect(Vector2 sourcePosition);
    void PlaySoundEffect(string effectName, Vector2 position);
    void PlayEffect(string effectName, Vector2 position);
    void PlayEffect(string effectName, Vector2 position, params object[] args);
}

internal class EffectsPlayer : IEffectsPlayer
{
    private readonly IGame _game;

    public EffectsPlayer(IGame game)
        => (_game) = (game);

    public void PlaySloMoEffect(TimeSpan slowMoDuration)
    {
        _game.RunCommand("/settime 0");

        Awaiter.Start(OnSlowMoEnded, slowMoDuration);
    }

    public void PlayShakeEffect(float intensity, TimeSpan duration)
    {
        _game.PlayEffect(EffectName.CameraShaker, Vector2.Zero, intensity, (float)duration.TotalMilliseconds, true);
        PlayShakeSoundEffect();
    }

    public void PlayShakeSoundEffect()
    {
        _game.PlaySound("Wings", Vector2.Zero, 1f);
    }

    public void PlayHandGleamEffect(IPlayer playerInstance)
    {
        var effectPosition = playerInstance.GetHandPosition();
        PlayEffect(EffectName.ItemGleam, effectPosition);
    }

    public void PlayLostEffect(Vector2 sourcePosition)
    {
        PlayEffect(EffectName.CustomFloatText,
            sourcePosition + new Vector2(SharedRandom.Instance.Next(-7, 7), SharedRandom.Instance.Next(-7, 7)), "?",
            ExtendedColors.Lost, 250f, 1f, true);
    }

    public void PlaySoundEffect(string effectName, Vector2 position)
    {
        _game.PlaySound(effectName, position, 1f);
    }

    private void OnSlowMoEnded()
    {
        _game.RunCommand("/settime 1");
        PlayShakeEffect(10f, TimeSpan.FromMilliseconds(250));
    }

    public void PlayEffect(string effectName, Vector2 position)
    {
        _game.PlayEffect(effectName, position);
    }

    public void PlayEffect(string effectName, Vector2 position, params object[] args)
    {
        _game.PlayEffect(effectName, position, args);
    }
}