using System;

namespace LuckyBlocks.Data;

internal interface IFinishCondition<out T>
{
    IFinishCondition<T> Invoke(Action<T> callback);
    void Dispose();
}