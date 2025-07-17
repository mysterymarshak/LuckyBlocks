using System;

namespace LuckyBlocks.Features.Buffs;

internal interface IFinishCondition<out T>
{
    IFinishCondition<T> Invoke(Action<T> callback);
    void Dispose();
}