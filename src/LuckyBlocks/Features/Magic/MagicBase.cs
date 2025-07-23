using System;
using Autofac;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic;

internal abstract class MagicBase : IMagic
{
    public abstract string Name { get; }
    public Player Wizard { get; }
    public IPlayer? WizardInstance => Wizard.Instance;
    public bool IsCloned { get; private set; }
    public bool IsFinished { get; private set; }
    public virtual bool ShouldCastOnRestore => true;
    public IFinishCondition<IMagic> WhenFinish => _finishCondition;

    protected ILifetimeScope LifetimeScope { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private readonly MagicFinishCondition _finishCondition;

    public MagicBase(Player wizard, MagicConstructorArgs args)
    {
        Wizard = wizard;
        LifetimeScope = args.LifetimeScope.BeginLifetimeScope();
        ExtendedEvents = LifetimeScope.Resolve<IExtendedEvents>();
        _finishCondition = new MagicFinishCondition();
    }

    public virtual IMagic Clone()
    {
        var clonedMagic = Copy();
        clonedMagic.IsCloned = true;
        return clonedMagic;
    }

    public void ExternalFinish()
    {
        if (IsFinished)
            return;

        OnFinish();
    }

    public abstract void Cast();
    public abstract MagicBase Copy();

    public void OnRestored()
    {
        IsCloned = false;
    }

    protected virtual void OnFinishInternal()
    {
    }

    private void OnFinish()
    {
        IsFinished = true;

        LifetimeScope.Dispose();
        ExtendedEvents.Clear();

        OnFinishInternal();
        SendFinishNotification();
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