using System;
using System.Collections.Generic;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Triggers;

internal interface ITriggersService
{
    void OnTriggerCallback(TriggerArgs args);
    IObjectScriptTrigger CreateHookForObject(IObject hookObject, Action<TriggerArgs> callback);
    void RemoveHookFromObject(IObject hookObject, Action<TriggerArgs> callback);
}

internal class TriggersService : ITriggersService
{
    private readonly ILogger _logger;
    private const string ScriptCallbackName = "ᅠ";
    private readonly IObjectScriptTrigger _scriptTrigger;
    private readonly Dictionary<IObject, List<Action<TriggerArgs>>> _hookedCallbacks = new();

    public TriggersService(IGame game, ILogger logger)
    {
        _logger = logger;
        _scriptTrigger = (IObjectScriptTrigger)game.CreateObject("ScriptTrigger");
        _scriptTrigger.SetScriptMethod(ScriptCallbackName);
        _scriptTrigger.CustomId = ScriptCallbackName;
    }

    public void OnTriggerCallback(TriggerArgs args)
    {
        try
        {
            var @object = args.Caller is IObjectTrigger { CustomId: not ScriptCallbackName } caller
                ? caller
                : args.Sender as IObject;

            if (@object is null || !_hookedCallbacks.TryGetValue(@object, out var callbacks))
                return;

            for (var index = callbacks.Count - 1; index >= 0; index--)
            {
                var callback = callbacks[index];
                callback.Invoke(args);
            }

            _logger.Verbose("Trigger callback; sender: '{Sender}', caller: '{Caller}'", args.Sender, args.Caller);
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in TriggersService.OnTriggerCallback");
        }
    }

    public IObjectScriptTrigger CreateHookForObject(IObject hookObject, Action<TriggerArgs> callback)
    {
        if (!_hookedCallbacks.TryGetValue(hookObject, out var callbacks))
        {
            callbacks = [];
            _hookedCallbacks.Add(hookObject, callbacks);
        }

        callbacks.Add(callback);

        return _scriptTrigger;
    }

    public void RemoveHookFromObject(IObject hookObject, Action<TriggerArgs> callback)
    {
        if (!_hookedCallbacks.TryGetValue(hookObject, out var callbacks))
            return;

        callbacks.Remove(callback);
    }
}