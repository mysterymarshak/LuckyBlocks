using System;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Identity;

namespace LuckyBlocks.Loot;

internal interface IBuffWrapper
{
    ILoot Wrap(Item item, IBuff buff, Player player);
}

internal class BuffWrapper : IBuffWrapper
{
    private readonly IBuffsService _buffsService;

    public BuffWrapper(IBuffsService buffsService)
        => (_buffsService) = (buffsService);

    public ILoot Wrap(Item item, IBuff buff, Player player)
    {
        return new WrappedBuff(item, buff, () => OnRan(buff, player));
    }

    private void OnRan(IBuff buff, Player player)
    {
        _buffsService.TryAddBuff(buff, player);
    }

    private class WrappedBuff : ILoot
    {
        public Item Item { get; }

        public string Name => _buff is IDurableBuff durableBuff
            ? $"{durableBuff.Name} | {durableBuff.Duration.TotalSeconds}s"
            : _buff.Name;

        private readonly IBuff _buff;
        private readonly Action _runCallback;

        public WrappedBuff(Item item, IBuff buff, Action runCallback)
            => (Item, _buff, _runCallback) = (item, buff, runCallback);

        public void Run()
        {
            _runCallback.Invoke();
        }
    }
}