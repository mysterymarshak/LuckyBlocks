namespace LuckyBlocks.Features.Keyboard;

internal interface IKeyboardEventSubscription
{
    int Id { get; }
    void ResetCooldown();
}