namespace LuckyBlocks.Wayback;

internal interface IStately<out T> where T : IState
{
    IState GetState();
    void RestoreFromState(IState state);
}

internal interface IState
{
}