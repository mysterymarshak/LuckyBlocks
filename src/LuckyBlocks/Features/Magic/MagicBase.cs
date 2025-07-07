using System;
using Autofac;
using LuckyBlocks.Data;

namespace LuckyBlocks.Features.Magic;

internal abstract class MagicBase : IMagic
{
    public abstract string Name { get; }

    public IFinishCondition<IMagic> WhenFinish => _finishCondition;

    protected ILifetimeScope LifetimeScope { get; }

    private readonly MagicFinishCondition _finishCondition;

    private bool _isFinished;

    public MagicBase(BuffConstructorArgs args)
        => (LifetimeScope, _finishCondition) = (args.LifetimeScope.BeginLifetimeScope(), new());

    public void ExternalFinish()
    {
        if (_isFinished)
            return;

        _isFinished = true;
        
        OnFinishedInternal();
        OnFinished();
        SendFinishNotification();
    }

    protected virtual void OnFinished()
    {
    }

    private void OnFinishedInternal()
    {
        LifetimeScope.Dispose();
    }

    private void SendFinishNotification()
    {
        _finishCondition.Callbacks?.Invoke(this);
        _finishCondition.Dispose();
    }

    private class MagicFinishCondition : IFinishCondition<IMagic>
    {
        public Action<IMagic>? Callbacks { get; private set; }

        public IFinishCondition<IMagic> Invoke(Action<IMagic> callback)
        {
            Callbacks += callback;
            return this;
        }

        public void Dispose()
        {
            Callbacks = null;
        }
    }
}