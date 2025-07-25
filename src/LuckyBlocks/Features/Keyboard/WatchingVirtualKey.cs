using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Keyboard;

internal class WatchingVirtualKey
{
    public VirtualKey Key { get; }
    public VirtualKeyEvent State { get; private set; }
    public VirtualKeyEvent LastState { get; private set; }
    public float LastStateChangedTime { get; private set; }

    public WatchingVirtualKey(VirtualKey key)
    {
        Key = key;
        State = VirtualKeyEvent.Released;
        LastState = VirtualKeyEvent.Released;
    }

    public void UpdateFromEvent(VirtualKeyInfo info, float elapsedTime)
    {
        LastState = State;
        State = info.Event;
        LastStateChangedTime = elapsedTime;
    }
}