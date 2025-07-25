using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

internal interface IEffectsPlayer
{
    void PlaySloMoEffect(TimeSpan slowMoDuration);
    void PlayShakeEffect(float intensity, TimeSpan duration);
    void PlayShakeSoundEffect();
    void PlayHandGleamEffect(IPlayer playerInstance);
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
        var effectPosition = playerInstance.GetWorldPosition() + new Vector2(0, 9) +
                             playerInstance.GetFaceDirection() * new Vector2(12, 0);
        PlayEffect(EffectName.ItemGleam, effectPosition);
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