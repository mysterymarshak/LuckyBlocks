using System;
using System.Diagnostics.CodeAnalysis;
using LuckyBlocks.Data;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using OneOf;
using OneOf.Types;

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

    private OneOf<Success, PlayerIsDeadResult, ImmunityFlag> OnRan(IBuff buff, Player player)
    {
        return _buffsService.TryAddBuff(buff, player, false);
    }

    private class WrappedBuff : ILoot
    {
        public Item Item { get; }

        [field: MaybeNull]
        public string Name => field ??= GetHintName();

        private readonly IBuff _buff;
        private readonly Func<OneOf<Success, PlayerIsDeadResult, ImmunityFlag>> _runCallback;

        private bool _isRepressed;

        public WrappedBuff(Item item, IBuff buff, Func<OneOf<Success, PlayerIsDeadResult, ImmunityFlag>> runCallback)
        {
            Item = item;
            _buff = buff;
            _runCallback = runCallback;
        }

        public void Run()
        {
            var buffAdditionResult = _runCallback.Invoke();
            if (buffAdditionResult.IsT2)
            {
                _isRepressed = true;
            }
        }

        private string GetHintName()
        {
            var name = _buff is IDurableBuff durableBuff
                ? $"{durableBuff.Name}: {durableBuff.Duration.TotalSeconds}s"
                : _buff.Name;

            return _isRepressed ? $"{name} | Repressed" : name;
        }
    }
}