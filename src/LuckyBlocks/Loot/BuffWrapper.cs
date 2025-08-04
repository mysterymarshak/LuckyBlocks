using System;
using System.Diagnostics.CodeAnalysis;
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
        _buffsService.TryAddBuff(buff, player, false);
    }

    private class WrappedBuff : ILoot
    {
        public Item Item { get; }

        [field: MaybeNull]
        public string Name => field ??= GetHintName();

        private readonly IBuff _buff;
        private readonly Action _runDelegate;

        public WrappedBuff(Item item, IBuff buff, Action runDelegate)
        {
            Item = item;
            _buff = buff;
            _runDelegate = runDelegate;
        }

        public void Run()
        {
            _runDelegate.Invoke();
        }

        private string GetHintName()
        {
            var name = _buff is IDurableBuff durableBuff
                ? $"{durableBuff.Name}: {durableBuff.Duration.TotalSeconds}s"
                : _buff.Name;

            return name;
        }
    }
}